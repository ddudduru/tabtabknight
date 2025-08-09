// Player_Control.cs
using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Player_Control : MonoBehaviour
{
    public static Player_Control Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Rigidbody rigidbodyComponent;
    [SerializeField] private Animator animator;
    [SerializeField] private BoxCollider attackRange;
    [SerializeField] private ParticleSystem hitObstacleEffect;
    [SerializeField] private BoxCollider skillRange;

    [Header("Movement / Stats")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] public float maxStamina = 100f;
    public float currentStamina;

    [Header("Acceleration")]
    [SerializeField] private float initialAcceleration = 7f;
    [SerializeField] private float maxAcceleration = 10f;
    private float currentAcceleration;
    private int moveDirection = 1;   // 1 or -1

    [Header("Skill / State")]
    public int maxSkillLevel = 5;
    private float forwardActive;       // 0 or 1
    public SkillController skillController;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 0.2f;
    private float attackTimer;
    private bool isAttacking;
    private bool attackReady;

    [Header("Dizzy")]
    private float dizzyTimer;
    private bool isDizzy;
    private bool isHit;

    [Header("UI / Effects")]
    [SerializeField] private GameObject scoreUpPrefab;
    [SerializeField] private TextMeshProUGUI staminaText;
    [SerializeField] private GameObject dashVFXPrefab;
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private GameObject areaAttackVFXPrefab;

    public bool IsDizzy => isDizzy;
    public bool IsHit => isHit;
    public float DizzyAmount => dizzyTimer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        currentStamina = maxStamina;
        currentAcceleration = initialAcceleration;
        forwardActive = 0;

        // SkillController �ʱ�ȭ �� ���
        skillController = gameObject.AddComponent<SkillController>();
        RegisterSkills();
    }

    private void Update()
    {
        HandleInput();
        UpdateTimers();
        HandleDizzyState();
        HandleMovement();
        HandleAttackRaycast();
        UpdateUI();
    }

    #region Input Handling

    private void HandleInput()
    {
        // ���� ��ȯ �Ǵ� dizzy ����
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (!isDizzy)
            {
                ToggleDirection();
            }
            else
            {
                dizzyTimer = Mathf.Max(0f, dizzyTimer - 1f);
            }
        }

        // 숫자 키(1~maxSkillLevel)로 스킬 사용 시도
        for (int lvl = 1; lvl <= maxSkillLevel; lvl++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + lvl))
            {
                if (!isDizzy && skillController.CanUseSkill(lvl, currentStamina))
                {
                    currentStamina = skillController.UseSkill(lvl, currentStamina);
                }
                else
                {
                    Debug.Log($"Stamina too low for Skill Level {lvl}");
                }
            }
        }

        // �ִϸ��̼� ��Ʈ�� (Q/W)
        if (Input.GetKeyDown(KeyCode.Q)) animator.SetBool("attack_rotate", true);
        if (Input.GetKeyDown(KeyCode.W)) animator.SetBool("attack_rotate", false);
    }

    private void ToggleDirection()
    {
        moveDirection *= -1;
        currentAcceleration = initialAcceleration;
    }

    #endregion

    #region Timers & State

    private void UpdateTimers()
    {
        attackTimer += Time.deltaTime;
        attackReady = (attackTimer >= attackCooldown);
        isDizzy = (dizzyTimer > 0f);
    }

    private void HandleDizzyState()
    {
        animator.SetBool("isDizzy", isDizzy);

        if (isDizzy)
        {
            rigidbodyComponent.linearVelocity = new Vector3(0f, 0f, GameManager.instance.gameSpd);
        }
    }

    private void EndSkill()
    {
        skillController.EndActiveSkill();
        UI_Control.instance.EndSkillTime();
        animator.SetBool("attack_rotate", false);
        skillRange.enabled = false;
    }

    #endregion

    #region Movement

    private void HandleMovement()
    {
        if (isDizzy) return;

        currentAcceleration = Mathf.Min(currentAcceleration + Time.deltaTime * 4f, maxAcceleration);

        Vector3 forwardVelocity = Vector3.zero;
        if (forwardActive > 0)
        {
            forwardVelocity = transform.forward * moveSpeed;
            forwardActive -= Time.deltaTime;
        }

        Vector3 lateralVelocity = new Vector3(moveDirection * currentAcceleration, 0f, 0f);
        Vector3 baseVelocity = Vector3.zero; // ���� ��� �ӵ� �ݿ��� �ʿ��ϸ� ����

        rigidbodyComponent.linearVelocity = forwardVelocity + lateralVelocity + baseVelocity;
        animator.SetBool("move_dir", moveDirection == 1);
    }

    #endregion

    #region Attack Handling

    private void HandleAttackRaycast()
    {
        if (isAttacking || isDizzy) return;

        Vector3 origin = transform.position + Vector3.up;
        Vector3[] directions = { transform.forward, Vector3.left, Vector3.right };

        foreach (var dir in directions)
        {
            if (Physics.Raycast(origin, dir, out var hit, 3f) &&
                hit.collider.CompareTag(ConstData.EnemyTag) && attackReady)
            {
                StartCoroutine(PerformSlash());
                break;
            }
        }
    }

    private IEnumerator PerformSlash()
    {
        attackTimer = 0f;
        isAttacking = true;

        int slashType = Random.Range(0, 2);
        animator.SetInteger("slashType", slashType);
        animator.SetTrigger("doSlash");
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Sword_Slash_Hit_Normal);

        yield return new WaitForSeconds(0.05f);

        attackRange.enabled = true;
        yield return new WaitForSeconds(0.1f);
        attackRange.enabled = false;

        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }

    #endregion

    #region Skill Registration

    private void RegisterSkills()
    {
        // 레벨 4: 플레이어 주변 AoE 지속 데미지 (지속형)
        {
            IContinuousEffectStrategy e4 = new AreaDamageEffect(
                damageAmount: 20f,
                radius: 5f,
                vfxKind: EffectPoolKind.AreaAttackVFX,
                soundEffect: SoundManager.SoundType.Effect_Rock_Break
            );
            IExecutionStrategy x4 = new PeriodicExecution(
                continuousEffect: e4,
                duration: 5f,
                interval: 0.01f
            );

            // 1) "doDash" 트리거
            var animparam1 = new AnimatorParameter("attack_rotate", AnimatorParameterType.Bool);
            animparam1.boolValue = true;
            // Animator 파라미터 묶음
            var animParams = new List<AnimatorParameter>()
            {
                animparam1
            };


            skillController.RegisterSkill(
                1,
                new CompositeSkill(
                    level: 1,
                    cost: 10f,
                    owner: gameObject,
                    execution: x4,
                    effect: e4,
                    animatorParams: animParams
                )
            );
        }


        // 이펙트: NearestEnemyDashEffect(데미지 30, 대쉬 거리 5, 탐색 반경 10, 전방 60도, VFX/Dash 사운드)
        {
            IEffectStrategy dashEffect = new NearestEnemyDashEffect(
                damageAmount: 30f,
                dashDistance: 5f,
                searchRadius: 10f,
                searchAngle: 180f,
                vfxKind: EffectPoolKind.DashVFX,
                soundEffect: SoundManager.SoundType.Sword_Slash_Hit_Normal
            );
            var exec = new SingleExecution(dashEffect);

            // AnimatorParam: Trigger("doDashToNearest") + Bool("isDashingNearest", true→끝나면 자동 false)
            var animParams = new List<AnimatorParameter>()
            {
                   new AnimatorParameter("doSkill_1",AnimatorParameterType.Trigger)
            };

            skillController.RegisterSkill(
                2,
                new CompositeSkill(
                    level: 1,
                    cost: 5f,
                    owner: gameObject,

                    execution: exec,
                    effect: dashEffect,
                    animatorParams: animParams
                )
            );
        }
    }

    #endregion

    #region Item & Obstacle Interaction

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ConstData.ItemTag) && other.TryGetComponent<Item>(out var itemComp))
        {
            HandleItemPickup(itemComp);
            itemComp.Despawn();
        }
        else if (other.CompareTag(ConstData.DeadZoneTag))
        {
            UI_Control.instance.FinishGame();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (forwardActive > 0 && other.CompareTag(ConstData.CannotGoZoneTag))
        {
            EndForward();
        }
    }

    /// <summary>
    /// 스태미나가 amount 이상이면 차감 후 true, 부족하면 false 반환.
    /// </summary>
    public bool TryUseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 외부에서 스태미나 회복이 필요할 때 호출 (예: 아이템 사용 시).
    /// </summary>
    public void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
    }



    private void HandleItemPickup(Item itemComp)
    {
        int scoreGain = 50;
        switch (itemComp.type)
        {
            case ItemType.Skill:
                currentStamina = Mathf.Min(maxStamina, currentStamina + 50f);
                break;
            case ItemType.Forward:
                forwardActive = 3; // �ణ ����
                break;
            case ItemType.Coin:
                scoreGain = 150;
                break;
        }
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);
        DisplayScorePopup(scoreGain);
    }

    public void HitObstacle(Obstacls_Control.Type type)
    {
        if (isHit) return;

        isAttacking = false;
        animator.SetTrigger("doDizzy");
        hitObstacleEffect.Play();

        float dizzyGain = 0f;
        switch (type)
        {
            case Obstacls_Control.Type.Tree:
                dizzyGain = 3f;
                break;
            case Obstacls_Control.Type.Log:
                dizzyGain = 7f;
                break;
            case Obstacls_Control.Type.Rock:
                dizzyGain = 5f;
                EndSkill();
                break;
        }
        dizzyTimer += dizzyGain;

        // 0.1�ʰ� �߰� �ǰ� ����
        isHit = true;
        Invoke(nameof(ResetHit), 0.1f);
    }

    public void HitPlayer(float addDizzyGain)
    {
        if (isHit) return;
        isAttacking = false;
        animator.SetTrigger("doDizzy");
        dizzyTimer += addDizzyGain;
        isHit = true;
        Invoke(nameof(ResetHit), 0.1f);
    }

    private void ResetHit()
    {
        isHit = false;
    }

    private void EndForward()
    {
        forwardActive = 0;
    }

    #endregion

    #region UI Helpers

    private void UpdateUI()
    {
        if (staminaText != null)
            staminaText.text = $"Stamina: {currentStamina:0}/{maxStamina:0}";
    }

    private void DisplayScorePopup(int score)
    {
        if (scoreUpPrefab == null) return;
        GameObject popup = Instantiate(
            scoreUpPrefab,
            transform.position + new Vector3(0f, 4f, 0f),
            Quaternion.Euler(50f, 0f, 0f)
        );
        var textComp = popup.transform.GetChild(0).GetChild(0)
                         .GetComponent<TextMeshProUGUI>();
        if (textComp != null) textComp.text = score.ToString();
        Destroy(popup, 1f);
    }

    #endregion
}
