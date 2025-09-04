using System.Collections;
using UnityEngine;

public class HomePlayer : MonoBehaviour
{
    [Header("Anim")]
    public Animator animator;
    public string startTrigger = "Start";

    [Header("Move")]
    public Vector3 targetPosition;          // world-space
    public float moveDuration = 1.0f;       // seconds
    public AnimationCurve easing = null;    // null => linear
    public bool rotateToDirection = true;
    public float rotateSpeedDeg = 720f;     // deg/sec

    Vector3 _startPos;
    bool _running;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        _startPos = transform.position;
    }

    public void StartGame()
    {
        if (_running)
        {
            return;
        }

        if (animator != null && !string.IsNullOrEmpty(startTrigger))
        {
            animator.SetTrigger(startTrigger);
        }

        StartCoroutine(Co_RunOut());
    }

    IEnumerator Co_RunOut()
    {
        yield return new WaitForSeconds(2f);
        _running = true;

        Vector3 p0 = transform.position;
        Vector3 p1 = targetPosition;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, moveDuration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = easing != null ? easing.Evaluate(t) : t;

            // position
            transform.position = Vector3.LerpUnclamped(p0, p1, k);

            // rotation (face movement direction)
            if (rotateToDirection)
            {
                Vector3 dir = (p1 - transform.position);
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        targetRot,
                        rotateSpeedDeg * Time.deltaTime
                    );
                }
            }

            yield return null;
        }

        // snap
        transform.position = p1;

        _running = false;

        // TODO: 여기에 실제 인게임 시작 로직 붙이면 딱 좋아요.
        // GameManager.instance.StartGame();
    }

    // 디버그용 가시화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, 0.2f);
        Gizmos.DrawLine(transform.position, targetPosition);
    }
}
