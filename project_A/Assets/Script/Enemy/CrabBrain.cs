// CrabBrain.cs
using UnityEngine;

public sealed class CrabBrain : IEnemyBrain
{
    private Enemy owner;
    private Transform player;

    // ������
    private float patrolWidth = 12f;
    private float lateralSpeed = 8f;
    private bool bounceOnWalls = true;
    private bool flipLocalScaleX = true;
    private string animFloatMovingSpeed = "movingSpeed";

    // ���� ����
    private int dir = +1;       // +1: ������, -1: ����
    private float startX, leftX, rightX;
    private Animator animator;

    public void Setup(Enemy owner, Transform player)
    {
        this.owner = owner;
        this.player = player;
        startX = owner.transform.position.x;
        float half = patrolWidth * 0.5f;
        leftX = startX - half;
        rightX = startX + half;

        animator = owner.GetComponentInChildren<Animator>();
        if (animator != null && !string.IsNullOrEmpty(animFloatMovingSpeed))
        {
            float randFloat = UnityEngine.Random.Range(1.8f, 2.2f);
            animator.SetFloat(animFloatMovingSpeed, randFloat);
        }
        owner.enemySpeed = 1f;
        UpdateFacing();
    }

    // Enemy�� ����� baseDelta(���� +Z ����)�� X�� �պ��� �� ��ȯ
    public Vector3 ModifyMove(Vector3 baseDelta, float dt)
    {
        if (lateralSpeed <= 0f || patrolWidth <= 0.01f)
            return baseDelta; // �¿� �̵� ��Ȱ��

        var tr = owner.transform;

        // ���� ������ X ���
        float xNow = tr.position.x;
        float xNext = xNow + (dir * lateralSpeed * dt);

        // ��� ���� �� ���� + ��迡 ����
        if (xNext <= leftX) { xNext = leftX; dir = +1; UpdateFacing(); }
        if (xNext >= rightX) { xNext = rightX; dir = -1; UpdateFacing(); }

        float lateralDelta = xNext - xNow;

        return baseDelta + new Vector3(lateralDelta, 0f, 0f);
    }

    public void OnHit()
    {
        // ������ ���׼�: ���� ����
        dir *= -1;
        UpdateFacing();
    }

    public void OnDespawn()
    {
        // ���� �ʱ�ȭ(Ǯ ���� ���)
        owner = null;
        player = null;
        animator = null;
    }

    public void OnTriggerEnter(Collider other)
    {
        // �¿� ����/�� �±׿� ������ ���� ����
        if (bounceOnWalls && other.CompareTag(ConstData.DeadZoneTag))
        {
            dir *= -1;
            UpdateFacing();
        }

        if (!other.CompareTag(ConstData.PlayerTag)) return;

        Player_Control.Instance.HitPlayer(2f);
    }

    private void UpdateFacing()
    {
        if (flipLocalScaleX && owner != null)
        {
            var s = owner.transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir >= 0 ? 1f : -1f);
            owner.transform.localScale = s;
        }
    }
}
