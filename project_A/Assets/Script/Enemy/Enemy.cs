using System;
using TMPro;
using UnityEngine;
using static SoundManager;

public class Enemy : MonoBehaviour
{
    public MonsterType monsterType;
    public GameObject scoreUpText;


    public Animator Anim { get; private set; }   // ← 프로퍼티 노출!
    private IEnemyBrain brain;
    private Transform player;
    public float enemySpeed = 1f;

    // 풀 매니저가 주입하는 반환 콜백
    public Action<Enemy> ReturnToPool { get; set; }

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        if (Anim != null)
            Anim.SetFloat("anim_spd", UnityEngine.Random.Range(0.7f, 1f));
    }

    // MapController(또는 풀 매니저)에서 스폰 직후 호출
    public void Initialize(MonsterType type, Transform playerTr)
    {
        monsterType = type;
        player = playerTr;

        brain = EnemyBrainFactory.Create(monsterType);
        brain?.Setup(this, player);
    }

    private void Update()
    {
        var baseDelta = new Vector3(0f, 0f, enemySpeed) * Time.deltaTime;
        var finalDelta = (brain != null)
            ? brain.ModifyMove(baseDelta, Time.deltaTime)
            : baseDelta;

        transform.position += finalDelta;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ConstData.AttackSlashTag))
        {
            HitEnemy();   // 점수/사운드 포함 일괄 처리
            return;
        }
        if (other.CompareTag(ConstData.DeadZoneETCTag))
        {
            Despawn();
            return;
        }

        //나머지 충돌은 브레인에게
        brain?.OnTriggerEnter(other);
    }


    public void HitEnemy()
    {
        int score = 0;
        SoundType sound = SoundType.None;

        switch (monsterType)
        {
            case MonsterType.Ghost:
                score = 20; sound = SoundType.Sword_Slash_Hit_Normal; break;
            case MonsterType.Skeleton:
                score = 30; sound = SoundType.Sword_Slash_Hit_Normal; break;
            case MonsterType.Bat:
                score = 15; sound = SoundType.Sword_Slash_Hit_Normal; break;
            case MonsterType.Slime:
                score = 5; sound = SoundType.Sword_Slash_Hit_Normal; break;
            case MonsterType.Crab:
                score = 10; sound = SoundType.Sword_Slash_Hit_Normal; break;
        }

        if (sound != SoundType.None)
            SoundManager.instance.Play_SoundEffect(sound);

        if (score > 0)
            GameManager.instance.PointUp(score);

        if (scoreUpText != null)
        {
            GameObject so = Instantiate(
                scoreUpText,
                transform.position + new Vector3(0f, 4f, 0f),
                Quaternion.Euler(50f, 0f, 0f)
            );
            var textComp = so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            if (textComp != null) textComp.text = score.ToString();
            Destroy(so, 1f);
        }

        brain?.OnHit();
        Despawn();
    }

    public void Despawn()
    {
        brain?.OnDespawn();

        if (ReturnToPool != null)
            ReturnToPool(this);
        else
            gameObject.SetActive(false); // 풀 미주입 시 폴백

        brain = null;
        player = null;
        ReturnToPool = null;
    }
}
