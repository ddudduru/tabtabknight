using UnityEngine;

/// <summary>
/// 위→아래 러너(예: movementDir=(0,0,-1)) 전제.
/// 플레이어는 화면 내 거의 고정. 맵/오브젝트가 흐르며 진행감을 주는 구조.
/// - 누적 이동거리(distanceTraveled)로 진행도/결승선 판단
/// - DeadZone은 뒤(Behind: -movementDir)에서 간격(deadGap) 유지/추격
/// - Goal은 앞(Ahead: +movementDir)에서 등장, 막판엔 항상 보이도록 고정
/// </summary>
public class LevelProgressManager3D : MonoBehaviour
{
    public static LevelProgressManager3D Instance { get; private set; }

    [Header("필수 참조")]
    [SerializeField] Transform player;         // 고정 주인공
    [SerializeField] Transform deadZone;       // 빨간선(즉사)
    [SerializeField] Transform goal;           // 결승선

    [Header("전진축 (화면 위→아래가 -Z이면 (0,0,-1))")]
    [SerializeField] Vector3 movementDir = new Vector3(0, 0, -1);

    [Header("속도 설정(거리/초)")]
    [SerializeField] float baseMoveSpeed = 5f;      // 맵 진행 속도 (= 플레이어 기준 진행속도)
    [SerializeField] float deadZoneSpeed = 4f;      // 데드존 기준 속도(난이도에 따라 플레이어보다 작거나/클게)
    [SerializeField, Range(0f, 1f)] float dizzySpeedFactor = 0.5f; // 기절 중 속도 비율

    [Header("거리/목표")]
    [SerializeField] float targetDistance = 1000f;        // 스테이지 총 거리
    [SerializeField, Range(0f, 1f)] float goalAppearAt = 0.80f; // 이 비율 이후 결승선 등장
    [SerializeField] float goalVisibleAhead = 6f;         // 막판 고정 가시거리(플레이어 앞)
    [SerializeField] float initialDeadGap = 20f;          // 시작 시 데드존과 간격(플레이어 뒤)

    [Header("옵션")]
    [SerializeField] bool hideGoalBeforeAppear = true;     // 나타나기 전 결승선 숨김

    // 런타임 상태
    public float distanceTraveled { get; private set; } = 0f;

    [SerializeField] bool freezeWorldOnDizzy = true;       // 기절 시 월드 정지 여부

    // 현재 속도
    public float CurrentSpeed
    {
        get
        {
            // 기절 중 완전 정지 옵션
            if (freezeWorldOnDizzy && Player_Control.Instance != null && Player_Control.Instance.IsDizzy)
            {
                return 0f;
            }

            float cur = baseMoveSpeed;

            // 정지 옵션을 끈 경우엔 느려지게만
            if (Player_Control.Instance != null && Player_Control.Instance.IsDizzy)
            {
                cur *= dizzySpeedFactor;
            }
            return cur;
        }
    }

    // 맵이 흘러야 하는 방향(플레이어 전진축의 반대)
    public Vector3 ScrollDir => -Fwd; // Fwd는 (movementDir.normalized)

    float deadGap;                    // 플레이어와 데드존 간 스칼라 간격(항상 양수)
    bool finished = false;

    // 전진축 노멀
    Vector3 Fwd => (movementDir.sqrMagnitude > 1e-6f) ? movementDir.normalized : Vector3.forward;

    public float TargetDistance => targetDistance;
    public float GoalAppearAt => goalAppearAt;
    public bool HideGoalBeforeAppear => hideGoalBeforeAppear;
    public Transform PlayerTr => player;
    public Transform DeadZoneTr => deadZone;
    public Transform GoalTr => goal;
    public Vector3 Forward => Fwd;
    public float CurrentDeadGap
    {
        get
        {
            if (PlayerTr == null || DeadZoneTr == null)
            {
                return 0f;
            }
            Vector3 delta = PlayerTr.position - DeadZoneTr.position;
            return Mathf.Max(0f, Vector3.Dot(delta, Forward));
        }
    }


    // ========== 새로 추가: 스테이지 리셋 API ==========
    /// <summary>
    /// 스테이지 시작/재시작/다음 스테이지 진입 시 호출.
    /// 전달한 값이 있으면 해당 파라미터를 덮어쓰고, null이면 기존 값 유지.
    /// - 맵/오브젝트 스크롤은 MapController가 담당 (여기선 진행도/데드존/골만 리셋)
    /// </summary>

    public void ResetForStage(StageMapDataSO data)
    {
        if (data == null)
        {
            // SO가 없으면 기존 값 유지 초기화
            ResetForStage(null, null, null, null, null, null, null);
            return;
        }

        ResetForStage(
            newTargetDistance: data.targetDistance,
            newBaseMoveSpeed: data.baseMoveSpeed,
            newDeadZoneSpeed: data.deadZoneSpeed,
            newGoalAppearAt: data.goalAppearAt,
            newGoalVisibleAhead: data.goalVisibleAhead,
            newInitialDeadGap: data.initialDeadGap,
            newHideGoalBeforeAppear: data.hideGoalBeforeAppear
        );
    }

    public void ResetForStage(
        float? newTargetDistance = null,
        float? newBaseMoveSpeed = null,
        float? newDeadZoneSpeed = null,
        float? newGoalAppearAt = null,
        float? newGoalVisibleAhead = null,
        float? newInitialDeadGap = null,
        bool? newHideGoalBeforeAppear = null
    )
    {
        // 1) 파라미터 오버라이드
        if (newTargetDistance.HasValue) { targetDistance = Mathf.Max(0f, newTargetDistance.Value); }
        if (newBaseMoveSpeed.HasValue) { baseMoveSpeed = Mathf.Max(0f, newBaseMoveSpeed.Value); }
        if (newDeadZoneSpeed.HasValue) { deadZoneSpeed = Mathf.Max(0f, newDeadZoneSpeed.Value); }
        if (newGoalAppearAt.HasValue)  { goalAppearAt = Mathf.Clamp01(newGoalAppearAt.Value); }
        if (newGoalVisibleAhead.HasValue) { goalVisibleAhead = Mathf.Max(0f, newGoalVisibleAhead.Value); }
        if (newInitialDeadGap.HasValue) { initialDeadGap = Mathf.Max(0f, newInitialDeadGap.Value); }
        if (newHideGoalBeforeAppear.HasValue) { hideGoalBeforeAppear = newHideGoalBeforeAppear.Value; }

        // 2) 런타임 값 리셋
        finished = false;
        distanceTraveled = 0f;
        deadGap = Mathf.Max(0f, initialDeadGap);

        // 3) 데드존/골 초기 배치 & 표시 상태
        if (deadZone != null && player != null)
        {
            // 뒤쪽(Behind) = -Fwd 방향으로 deadGap만큼
            deadZone.position = player.position - Fwd * deadGap;
        }

        if (goal != null)
        {
            if (hideGoalBeforeAppear)
            {
                goal.gameObject.SetActive(false);
            }
            else
            {
                goal.gameObject.SetActive(true);
                // 시작부터 보이게 할 경우 플레이어 앞에 최소 가시거리로 위치
                goal.position = player.position + Fwd * goalVisibleAhead;
            }
        }
    }
    // ===============================================

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else if (Instance != this) { Destroy(gameObject); return; }
        // DontDestroyOnLoad를 원하면 주석 해제
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        deadGap = Mathf.Max(0f, initialDeadGap);

        if (goal != null && hideGoalBeforeAppear)
        {
            goal.gameObject.SetActive(false);
        }

        // 시작 위치 정렬(선택)
        if (deadZone != null && player != null)
        {
            deadZone.position = player.position - Fwd * deadGap;
        }
    }

    void Update()
    {
        if (!GameManager.instance.isStart)
        {
            return;
        }

        if (finished || Player_Control.Instance.IsDead)
        {
            return;
        }

        float curSpeed = CurrentSpeed;
        float dz = curSpeed * Time.deltaTime;
        distanceTraveled += dz;

        // 3) 클리어 판정
        if (distanceTraveled >= targetDistance)
        {
            finished = true;
            OnFinishReached();
            return;
        }

        // 4) 데드존-플레이어 간격 업데이트(추격/이탈)
        if (curSpeed > deadZoneSpeed)
        {
            deadGap += (curSpeed - deadZoneSpeed) * Time.deltaTime; // 점점 멀어짐(안전)
        }
        else if (curSpeed < deadZoneSpeed)
        {
            deadGap -= (deadZoneSpeed - curSpeed) * Time.deltaTime; // 따라붙음(위험)
        }

        // 5) 데드존 접촉(즉사)
        if (deadGap <= 0f)
        {
            OnDeadZoneHit();
            return;
        }

        // 6) 데드존 위치 갱신(뒤쪽)
        if (deadZone != null && player != null)
        {
            deadZone.position = player.position - Fwd * deadGap;
        }

        // 7) 결승선 등장/위치 갱신(앞쪽)
        float t = Mathf.Clamp01(distanceTraveled / targetDistance);
        if (goal != null)
        {
            // 등장
            if (!goal.gameObject.activeSelf && t >= goalAppearAt)
            {
                goal.gameObject.SetActive(true);
            }

            if (goal.gameObject.activeSelf)
            {
                // 남은 거리
                float remain = Mathf.Max(0f, targetDistance - distanceTraveled);

                // 막판엔 항상 화면 안: 앞쪽 최소 가시거리 보장
                float aheadDist = Mathf.Max(goalVisibleAhead, remain);

                // 플레이어 앞(+Fwd)으로 배치
                if (player != null)
                {
                    goal.position = player.position + Fwd * aheadDist;
                }
            }
        }
    }

    /// <summary>
    /// Forward 아이템/대시 등으로 "추가 전진"이 발생했을 때 호출.
    /// 누적 거리와 데드존 간격을 함께 늘려 추격 밸런스 유지.
    /// </summary>
    public void AddDistance(float extra)
    {
        if (finished || Player_Control.Instance.IsDead) { return; }
        if (extra <= 0f) { return; }

        distanceTraveled += extra;
        deadGap += extra; // 플레이어가 앞으로 확 당기면 데드존과 간격도 벌어짐

        // 결승선 즉시 체크
        if (distanceTraveled >= targetDistance)
        {
            finished = true;
            OnFinishReached();
            return;
        }

        // 결승선 등장 조건 재평가
        if (goal != null && hideGoalBeforeAppear)
        {
            float t = Mathf.Clamp01(distanceTraveled / targetDistance);
            if (!goal.gameObject.activeSelf && t >= goalAppearAt)
            {
                goal.gameObject.SetActive(true);
            }
        }
    }

    void OnFinishReached()
    {
        Debug.Log("[LevelProgress] FINISH!");
        GameStateMachine.Instance.OnStageResult(true);
    }

    void OnDeadZoneHit()
    {
        Debug.Log("[LevelProgress] DEAD by DeadZone");
        // 즉사 처리
        Player_Control.Instance.CurrentHP = 0f;
    }

    // 디버그용 가시선
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (player == null) { return; }
        var f = (movementDir.sqrMagnitude > 1e-6f) ? movementDir.normalized : Vector3.forward;

        Gizmos.color = Color.red;   // DeadZone 예상선
        Gizmos.DrawLine(player.position - f * (initialDeadGap * 0.5f),
                        player.position - f * (initialDeadGap * 1.5f));

        Gizmos.color = Color.cyan;  // Goal 예상선
        Gizmos.DrawLine(player.position + f * (goalVisibleAhead),
                        player.position + f * (goalVisibleAhead + 10f));
    }
#endif
}
