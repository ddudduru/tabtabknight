using System.Collections;
using UnityEngine;

public class StartSequenceController : MonoBehaviour
{
    [Header("Player")]
    public Transform player;                  // required
    public Animator playerAnimator;           // optional
    public string playerStartTrigger = "Start";
    public Vector3 playerTargetPosition;      // world-space
    public float playerMoveDuration = 1.0f;   // seconds
    public AnimationCurve playerEasing = null; // set to Linear in OnValidate
    public bool playerRotateToDirection = true;
    public float playerRotateSpeedDeg = 720f;

    [Header("Cameras (Enable/Disable Only)")]
    public Camera cameraHome;                 // optional
    public Camera cameraIngame;               // optional
    public bool switchToIngameOnComplete = true;

    [Header("Timing / Flags")]
    public float playerStartDelay = 0.0f;     // delay before player starts moving
    public float switchDelay = 0.25f;         // delay after player finishes before switch
    public bool setGameStartFlag = true;

    private bool _running;

    private void Awake()
    {
        // Home on, Ingame off (if assigned)
        if (cameraHome != null)
        {
            cameraHome.gameObject.SetActive(true);
        }
        if (cameraIngame != null)
        {
            cameraIngame.gameObject.SetActive(false);
        }
    }

    public void Begin()
    {
        if (_running)
        {
            return;
        }

        if (player == null)
        {
            Debug.LogError("[StartSequenceLite] Missing player Transform.");
            return;
        }

        // Fire player anim trigger early (non-blocking)
        if (playerAnimator != null && !string.IsNullOrEmpty(playerStartTrigger))
        {
            playerAnimator.SetTrigger(playerStartTrigger);
        }

        _running = true;
        StartCoroutine(Co_Run());
    }

    private IEnumerator Co_Run()
    {
        if (playerStartDelay > 0f)
        {
            yield return new WaitForSeconds(playerStartDelay);
        }

        Vector3 p0 = player.position;
        Vector3 p1 = playerTargetPosition;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, playerMoveDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = playerEasing != null ? playerEasing.Evaluate(t) : t;

            // position
            player.position = Vector3.LerpUnclamped(p0, p1, k);

            // rotation (face move direction)
            if (playerRotateToDirection)
            {
                Vector3 dir = (p1 - player.position);
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    player.rotation = Quaternion.RotateTowards(
                        player.rotation,
                        targetRot,
                        playerRotateSpeedDeg * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // snap to final
        player.position = p1;

        if (switchDelay > 0f)
        {
            yield return new WaitForSeconds(switchDelay);
        }

        if (switchToIngameOnComplete)
        {
            if (cameraHome != null)
            {
                cameraHome.gameObject.SetActive(false);
            }
            if (cameraIngame != null)
            {
                cameraIngame.gameObject.SetActive(true);
            }
        }

        if (setGameStartFlag && GameManager.instance != null)
        {
            GameManager.instance.isStart = true;
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
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTargetPosition, 0.2f);
            Gizmos.DrawLine(player.position, playerTargetPosition);
        }
    }
#endif
}
