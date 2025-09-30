using System.Collections;
using UnityEngine;

public class StartSequenceController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;                  // required
    public Animator playerAnimator;           // optional
    [Tooltip("Start 시점에 한 번 쏘는 트리거(선택)")]
    public string playerStartTrigger = "Start";
    [Tooltip("Return 시점에 한 번 쏘는 트리거(선택)")]
    public string playerReturnTrigger = "Return";

    [Header("Animator Params (Optional)")]
    [Tooltip("앉기 트리거 파라미터 이름 (없으면 공백)")]
    public string sitTriggerParam = "Sit";
    [Tooltip("앉기 상태 대기용 태그(없으면 대기 안 함)")]
    public string standToSitStateTag = "StandToSit";
    public string sitToStandStateTag = "SitToStand";
    [Tooltip("Sit 트리거 후 최대 대기 (초)")]
    public float sitWaitTimeout = 2.0f;
    [Tooltip("앉기 완료 후 추가 대기 (초)")]
    public float afterSitDelay = 0.25f;

    [Header("Positions")]
    public Vector3 playerStartTargetPosition;   // world
    public Vector3 playerReturnTargetPosition;  // world

    [Header("Move / Rotate")]
    public float playerMoveDuration = 1.0f;     // 이동 시간
    public AnimationCurve playerEasing = null;  // 기본 Linear
    public bool playerRotateToDirection = true; // 이동 중 방향보기
    public float playerRotateSpeedDeg = 720f;   // 이동 중 회전 속도
    [Tooltip("도착 후 제자리 뒤돌기 사용")]
    public bool rotateBackwardsOnReturn = true;
    [Tooltip("도착 후 제자리 회전 속도(초당)")]
    public float rotateInPlaceSpeedDeg = 360f;
    [Tooltip("Return 뒤돌 때 회전 방향: 체크=시계(-Y), 해제=반시계(+Y)")]
    public bool returnTurnClockwise = true;

    [Header("Cameras (Enable/Disable Only)")]
    public Camera cameraHome;                 // optional
    public Camera cameraIngame;               // optional
    public bool switchToIngameOnComplete = true;

    [Header("Timing / Flags")]
    public float playerStartDelay = 0.0f;     // Start 이동 전 딜레이
    public float switchDelay = 0.25f;         // Start 이동 후 카메라 전환까지 딜레이
    public bool setGameStartFlag = true;      // Start 완료 시 GameManager.isStart = true
    [Tooltip("Return 시작 시 GameManager.isStart = false로 내림")]
    public bool clearGameStartOnReturn = true;

    private bool _running;
    private bool _isSit;
    public static StartSequenceController Instance = null;

    Coroutine startCoroutine = null;
    Coroutine returnCoroutine = null;

    private void ResetCoroutine()
    {
        if (startCoroutine != null)
        {
            StopCoroutine(startCoroutine);
        }

        if (returnCoroutine != null) 
        {
            StopCoroutine(returnCoroutine);
        }

    }


    private void Awake()
    {
        if (cameraHome != null) { cameraHome.gameObject.SetActive(true); }
        if (cameraIngame != null) { cameraIngame.gameObject.SetActive(false); }

        Instance = this;
    }

    public void Begin()
    {
        ResetCoroutine();

        if (player == null)
        {
            Debug.LogError("[StartSequence] Missing player Transform.");
            return;
        }

        if (playerAnimator != null && !string.IsNullOrEmpty(playerStartTrigger))
        {
            playerAnimator.SetTrigger(playerStartTrigger);
        }

        _running = true;
        startCoroutine = StartCoroutine(CoStartRun());
    }

    public void Sit()
    {
        if (playerAnimator != null && !string.IsNullOrEmpty(sitTriggerParam))
        {
            playerAnimator.SetTrigger(sitTriggerParam);
            playerAnimator.SetBool("isSit", true);
        }
    }

    public void Return()
    {
        ResetCoroutine();

        if (player == null)
        {
            Debug.LogError("[StartSequence] Missing player Transform.");
            return;
        }

        if (clearGameStartOnReturn && GameManager.instance != null)
        {
            GameManager.instance.isStart = false;
        }

        if (playerAnimator != null && !string.IsNullOrEmpty(playerReturnTrigger))
        {
            playerAnimator.SetTrigger(playerReturnTrigger);
        }

        _running = true;
        returnCoroutine = StartCoroutine(CoReturnRun());
    }

    private IEnumerator CoReturnRun()
    {
        // 홈 카메라 On, 인게임 Off
        if (cameraHome != null) { cameraHome.gameObject.SetActive(true); }
        if (cameraIngame != null) { cameraIngame.gameObject.SetActive(false); }
        
        // 1) 현재 위치 -> Return 타겟까지 이동
        Vector3 p0 = playerStartTargetPosition;
        //player.position = p0;
       Vector3 p1 = playerReturnTargetPosition;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, playerMoveDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = (playerEasing != null) ? playerEasing.Evaluate(t) : t;

            // position
            player.position = Vector3.LerpUnclamped(p0, p1, k);

            // face move direction
            if (playerRotateToDirection)
            {
                Vector3 dir = (p1 - player.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRotMove = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    player.rotation = Quaternion.RotateTowards(
                        player.rotation, targetRotMove, playerRotateSpeedDeg * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // 스냅
        player.position = p1;

        // 2) 도착 후 제자리 뒤돌기(지정한 방향으로만 180° 회전)
        if (rotateBackwardsOnReturn)
        {
            // 최종 목표: 현재에서 180° 뒤 방향
            Quaternion targetRot = Quaternion.AngleAxis(180f, Vector3.up) * player.rotation;

            // 회전 부호(시계: -1, 반시계: +1), 월드 Y 축 기준
            float sign = returnTurnClockwise ? -1f : +1f;

            float remaining = 180f;
            while (remaining > 0.1f)
            {
                float step = Mathf.Min(rotateInPlaceSpeedDeg * Time.deltaTime, remaining);
                player.Rotate(0f, sign * step, 0f, Space.World);
                remaining -= step;
                yield return null;
            }

            // 스냅 보정
            player.rotation = targetRot;
        }

        // 3) 앉기 트리거 & (선택) 상태 대기
        if (playerAnimator != null && !string.IsNullOrEmpty(sitTriggerParam))
        {
            playerAnimator.SetTrigger(sitTriggerParam);

            if (!string.IsNullOrEmpty(sitToStandStateTag))
            {
                float timer = 0f;
                while (timer < sitWaitTimeout)
                {
                    var st = playerAnimator.GetCurrentAnimatorStateInfo(0);
                    if (st.IsTag(sitToStandStateTag)) { break; }
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }

        if (afterSitDelay > 0f)
        {
            yield return new WaitForSeconds(afterSitDelay);
        }

        _running = false;
    }


    private IEnumerator CoStartRun()
    {
        yield return new WaitForSeconds(0.25f);

        if (playerAnimator != null)
        {
            if (!string.IsNullOrEmpty(standToSitStateTag))
            {
                float timer = 0f;
                while (timer < sitWaitTimeout)
                {
                    var st = playerAnimator.GetCurrentAnimatorStateInfo(0);
                    if (!st.IsTag(standToSitStateTag)) { break; }
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }

        if (playerAnimator != null)
        {
            if (!string.IsNullOrEmpty(sitToStandStateTag))
            {
                float timer = 0f;
                while (timer < sitWaitTimeout)
                {
                    var st = playerAnimator.GetCurrentAnimatorStateInfo(0);
                    if (!st.IsTag(sitToStandStateTag)) { break; }
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }




        Vector3 p0 = player.position;
        //player.position = p0;
       Vector3 p1 = playerStartTargetPosition;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, playerMoveDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = (playerEasing != null) ? playerEasing.Evaluate(t) : t;

            player.position = Vector3.LerpUnclamped(p0, p1, k);

            if (playerRotateToDirection)
            {
                Vector3 dir = (p1 - player.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    player.rotation = Quaternion.RotateTowards(
                        player.rotation, targetRot, playerRotateSpeedDeg * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // 스냅
        player.position = p1;

        // 카메라 전환 딜레이
        if (switchDelay > 0f) { yield return new WaitForSeconds(switchDelay); }

        if (switchToIngameOnComplete)
        {
            if (cameraHome != null) { cameraHome.gameObject.SetActive(false); }
            if (cameraIngame != null) { cameraIngame.gameObject.SetActive(true); }
        }

        if (setGameStartFlag && GameManager.instance != null)
        {
            GameManager.instance.isStart = true;
            if (playerAnimator != null)
            {
                playerAnimator.Rebind();
            }
        }

        _running = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (playerEasing == null)
        {
            playerEasing = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
        sitWaitTimeout = Mathf.Max(0f, sitWaitTimeout);
        afterSitDelay = Mathf.Max(0f, afterSitDelay);
    }
#endif
}
