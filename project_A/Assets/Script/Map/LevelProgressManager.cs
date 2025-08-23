using UnityEngine;

/// <summary>
/// ����Ʒ� ����(��: movementDir=(0,0,-1)) ����.
/// �÷��̾�� ȭ�� �� ���� ����. ��/������Ʈ�� �帣�� ���న�� �ִ� ����.
/// - ���� �̵��Ÿ�(distanceTraveled)�� ���൵/��¼� �Ǵ�
/// - DeadZone�� ��(Behind: -movementDir)���� ����(deadGap) ����/�߰�
/// - Goal�� ��(Ahead: +movementDir)���� ����, ���ǿ� �׻� ���̵��� ����
/// </summary>
public class LevelProgressManager3D : MonoBehaviour
{
    [Header("�ʼ� ����")]
    [SerializeField] Transform player;         // ���� ���ΰ�
    [SerializeField] Transform deadZone;       // ������(���)
    [SerializeField] Transform goal;           // ��¼�

    [Header("������ (ȭ�� ����Ʒ��� -Z�̸� (0,0,-1))")]
    [SerializeField] Vector3 movementDir = new Vector3(0, 0, -1);

    [Header("�ӵ� ����(�Ÿ�/��)")]
    [SerializeField] float baseMoveSpeed = 5f;      // �� ���� �ӵ� (= �÷��̾� ���� ����ӵ�)
    [SerializeField] float deadZoneSpeed = 4f;      // ������ ���� �ӵ�(���̵��� ���� �÷��̾�� �۰ų�/Ŭ��)
    [SerializeField, Range(0f, 1f)] float dizzySpeedFactor = 0.5f; // ���� �� �ӵ� ����

    [Header("�Ÿ�/��ǥ")]
    [SerializeField] float targetDistance = 1000f;        // �������� �� �Ÿ�
    [SerializeField, Range(0f, 1f)] float goalAppearAt = 0.80f; // �� ���� ���� ��¼� ����
    [SerializeField] float goalVisibleAhead = 6f;         // ���� ���� ���ðŸ�(�÷��̾� ��)
    [SerializeField] float initialDeadGap = 20f;          // ���� �� �������� ����(�÷��̾� ��)

    [Header("�ɼ�")]
    [SerializeField] bool hideGoalBeforeAppear = true;     // ��Ÿ���� �� ��¼� ����

    // ��Ÿ�� ����
    public float distanceTraveled { get; private set; } = 0f; // ���� ����Ÿ�
    float deadGap;                    // �÷��̾�� ������ �� "��Į��" ����(���� ����, �׻� ���)
    bool finished = false;
    bool dead = false;

    // ������ ���
    Vector3 Fwd => (movementDir.sqrMagnitude > 1e-6f) ? movementDir.normalized : Vector3.forward;

    void Start()
    {
        deadGap = Mathf.Max(0f, initialDeadGap);

        if (goal)
        {
            if (hideGoalBeforeAppear) goal.gameObject.SetActive(false);
        }

        // ���� ��ġ ����(����)
        if (deadZone && player)
        {
            // ����(Behind) = -Fwd �������� deadGap��ŭ
            deadZone.position = player.position - Fwd * deadGap;
        }
    }

    void Update()
    {
        if (finished || dead) return;

        // 1) ���� ����ӵ�(���� ����)
        float curSpeed = baseMoveSpeed;
        if (Player_Control.Instance && Player_Control.Instance.IsDizzy)
            curSpeed *= dizzySpeedFactor;

        // 2) ���� �Ÿ�
        float dz = curSpeed * Time.deltaTime;
        distanceTraveled += dz;

        // 3) Ŭ���� ����
        if (distanceTraveled >= targetDistance)
        {
            finished = true;
            OnFinishReached();
            return;
        }

        // 4) ������-�÷��̾� ���� ������Ʈ(�߰�/��Ż)
        if (curSpeed > deadZoneSpeed)
            deadGap += (curSpeed - deadZoneSpeed) * Time.deltaTime; // ���� �־���(����)
        else if (curSpeed < deadZoneSpeed)
            deadGap -= (deadZoneSpeed - curSpeed) * Time.deltaTime; // �������(����)

        // 5) ������ ����(���)
        if (deadGap <= 0f)
        {
            dead = true;
            OnDeadZoneHit();
            return;
        }

        // 6) ������ ��ġ ����(����)
        if (deadZone && player)
            deadZone.position = player.position - Fwd * deadGap;

        // 7) ��¼� ����/��ġ ����(����)
        float t = Mathf.Clamp01(distanceTraveled / targetDistance);
        if (goal)
        {
            // ����
            if (!goal.gameObject.activeSelf && t >= goalAppearAt)
                goal.gameObject.SetActive(true);

            if (goal.gameObject.activeSelf)
            {
                // ���� �Ÿ�
                float remain = Mathf.Max(0f, targetDistance - distanceTraveled);

                // ���ǿ� �׻� ȭ�� ��: ���� �ּ� ���ðŸ� ����
                float aheadDist = Mathf.Max(goalVisibleAhead, remain);

                // �÷��̾� ��(+Fwd)���� ��ġ
                goal.position = player.position + Fwd * aheadDist;
            }
        }
    }

    /// <summary>
    /// Forward ������/��� ������ "�߰� ����"�� �߻����� �� ȣ��.
    /// ���� �Ÿ��� ������ ������ �Բ� �÷� �߰� �뷱�� ����.
    /// </summary>
    public void AddDistance(float extra)
    {
        if (finished || dead) return;
        if (extra <= 0f) return;

        distanceTraveled += extra;
        deadGap += extra; // �÷��̾ ������ Ȯ ���� �������� ���ݵ� ������

        // ��¼� ��� üũ
        if (distanceTraveled >= targetDistance)
        {
            finished = true;
            OnFinishReached();
            return;
        }

        // ��¼� ���� ���� ����
        if (goal && hideGoalBeforeAppear)
        {
            float t = Mathf.Clamp01(distanceTraveled / targetDistance);
            if (!goal.gameObject.activeSelf && t >= goalAppearAt)
                goal.gameObject.SetActive(true);
        }
    }

    void OnFinishReached()
    {
        // ����/�� ���� �� ����
        Debug.Log("[LevelProgress] FINISH!");
        // ����: MapController.SetWorldSpeed(0f);
        // ����: UI_Control.instance.FinishGame();
    }

    void OnDeadZoneHit()
    {
        Debug.Log("[LevelProgress] DEAD by DeadZone");
        // ��� ó��
        // ����: UI_Control.instance.FinishGame();
        // �Ǵ� Player_Control.Instance.HitPlayer(9999f);
    }

    // ����׿� ���ü�
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!player) return;
        var f = (movementDir.sqrMagnitude > 1e-6f) ? movementDir.normalized : Vector3.forward;

        Gizmos.color = Color.red;   // DeadZone ����
        Gizmos.DrawLine(player.position - f * (initialDeadGap * 0.5f),
                        player.position - f * (initialDeadGap * 1.5f));

        Gizmos.color = Color.cyan;  // Goal ����
        Gizmos.DrawLine(player.position + f * (goalVisibleAhead),
                        player.position + f * (goalVisibleAhead + 10f));
    }
#endif
}
