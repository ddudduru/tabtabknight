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
    public float distanceTraveled { get; private set; } = 0f; // 누적 진행거리
    float deadGap;                    // 플레이어와 데드존 간 "스칼라" 간격(월드 단위, 항상 양수)
    bool finished = false;
    bool dead = false;

    // 전진축 노멀
    Vector3 Fwd => (movementDir.sqrMagnitude > 1e-6f) ? movementDir.normalized : Vector3.forward;

    void Start()
    {
        deadGap = Mathf.Max(0f, initialDeadGap);

        if (goal)
        {
            if (hideGoalBeforeAppear) goal.gameObject.SetActive(false);
        }

        // 시작 위치 정렬(선택)
        if (deadZone && player)
        {
            // 뒤쪽(Behind) = -Fwd 방향으로 deadGap만큼
            deadZone.position = player.position - Fwd * deadGap;
        }
    }

    void Update()
    {
        if (finished || dead) return;

        // 1) 현재 진행속도(기절 적용)
        float curSpeed = baseMoveSpeed;
        if (Player_Control.Instance && Player_Control.Instance.IsDizzy)
            curSpeed *= dizzySpeedFactor;

        // 2) 누적 거리
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
            deadGap += (curSpeed - deadZoneSpeed) * Time.deltaTime; // 점점 멀어짐(안전)
        else if (curSpeed < deadZoneSpeed)
            deadGap -= (deadZoneSpeed - curSpeed) * Time.deltaTime; // 따라붙음(위험)

        // 5) 데드존 접촉(즉사)
        if (deadGap <= 0f)
        {
            dead = true;
            OnDeadZoneHit();
            return;
        }

        // 6) 데드존 위치 갱신(뒤쪽)
        if (deadZone && player)
            deadZone.position = player.position - Fwd * deadGap;

        // 7) 결승선 등장/위치 갱신(앞쪽)
        float t = Mathf.Clamp01(distanceTraveled / targetDistance);
        if (goal)
        {
            // 등장
            if (!goal.gameObject.activeSelf && t >= goalAppearAt)
                goal.gameObject.SetActive(true);

            if (goal.gameObject.activeSelf)
            {
                // 남은 거리
                float remain = Mathf.Max(0f, targetDistance - distanceTraveled);

                // 막판엔 항상 화면 안: 앞쪽 최소 가시거리 보장
                float aheadDist = Mathf.Max(goalVisibleAhead, remain);

                // 플레이어 앞(+Fwd)으로 배치
                goal.position = player.position + Fwd * aheadDist;
            }
        }
    }

    /// <summary>
    /// Forward 아이템/대시 등으로 "추가 전진"이 발생했을 때 호출.
    /// 누적 거리와 데드존 간격을 함께 늘려 추격 밸런스 유지.
    /// </summary>
    public void AddDistance(float extra)
    {
        if (finished || dead) return;
        if (extra <= 0f) return;

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
        if (goal && hideGoalBeforeAppear)
        {
            float t = Mathf.Clamp01(distanceTraveled / targetDistance);
            if (!goal.gameObject.activeSelf && t >= goalAppearAt)
                goal.gameObject.SetActive(true);
        }
    }

    void OnFinishReached()
    {
        // 월드/맵 정지 등 연출
        Debug.Log("[LevelProgress] FINISH!");
        // 예시: MapController.SetWorldSpeed(0f);
        // 예시: UI_Control.instance.FinishGame();
    }

    void OnDeadZoneHit()
    {
        Debug.Log("[LevelProgress] DEAD by DeadZone");
        // 즉사 처리
        // 예시: UI_Control.instance.FinishGame();
        // 또는 Player_Control.Instance.HitPlayer(9999f);
    }

    // 디버그용 가시선
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!player) return;
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
