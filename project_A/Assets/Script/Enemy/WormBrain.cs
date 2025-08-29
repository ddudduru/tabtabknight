// ---------------------------------------------------
// WormBrain.cs
// IEnemyBrain ���: ����(Hidden) �� ����(Grounded) ���� + ���Ÿ� ���
// ---------------------------------------------------
using System.Collections;
using UnityEngine;

public class WormBrain : IEnemyBrain
{
    private enum State { Hidden, Grounded }

    private Enemy owner;
    private Transform player;
    private Transform parentForScroll; // ���� �� ��Ʈ(��ũ�� ���)

    // ===== Ž��/���� �Ķ���� =====
    [Header("Ž��/����")]
    private float detectRange = 40f;     // �� ���ϸ� �������� �ö��(Grounded)
    private float exitRange = 50f;     // �� �̻� �־����� �ٽ� ���(Hidden) (�����׸��ý�)
    private float fireRange = 100f;     // ��� ������ �Ÿ�
    private float fireAngle = 55f;     // ���� ���� ��� ��� ����
    private bool requireLOS = true;    // �ܼ� �þ�(����) üũ

    private float fireCooldown = 1f;   // �� �� ��� ���� �߱��� ���
    private float fireTimer = 0f;

    // ===== ����/��Ʈ�ڽ� =====
    private float burrowDepth = 0.5f;    // ���� �� ���� �Ʒ��� ������ ����(�����)
    private Vector3 baseLocalPos;        // ���� ���� ���� ��ġ(���� ��ġ)
    private Renderer[] rends;
    private Collider[] cols;

    // ===== ����ü =====
    private float projSpeed = 10f;
    private float projLife = 3.5f;
    private float projDizzyOnHit = 2.5f; // �÷��̾� �ǰ� �� ������ ������
    private float muzzleYOffset = 0.7f; // �ѱ� ����

    private State state;

    // ========= IEnemyBrain ���� =========

    public void Setup(Enemy owner, Transform player)
    {
        this.owner = owner;
        this.player = player;

        parentForScroll = owner.transform.parent;

        baseLocalPos = owner.transform.localPosition;
        CacheComponents();

        // ó���� ���� ���·� ����
        SetState(State.Hidden, instant: true);
        fireTimer = 0f;
    }

    public Vector3 ModifyMove(Vector3 baseDelta, float dt)
    {
        baseDelta = Vector3.zero;
        if (player == null) return baseDelta;

        // XZ ��� �Ÿ� / ����
        Vector3 self = owner.transform.position;
        Vector3 toP = player.position - self;
        Vector3 toPFlat = new Vector3(toP.x, 0f, toP.z);
        float dist = toPFlat.magnitude;

        // ���� ����
        switch (state)
        {
            case State.Hidden:
                // �÷��̾ Ž�� ���� ������ ������ �������� ����
                if (dist <= detectRange)
                {
                    SetState(State.Grounded);
                    owner.Anim.SetTrigger("doBreakThrough");
                }
                break;

            case State.Grounded:
                // �ʹ� �־����� �ٽ� ����
                if (dist >= exitRange)
                {
                    SetState(State.Hidden);
                    owner.Anim.SetTrigger("doDive");
                    break;
                }

                // �÷��̾� �������� õõ�� �� ������(����)
                FaceTowards(toPFlat, dt);

                // ��� ���� �˻�
                fireTimer += dt;

                bool inRange = dist <= fireRange;
                bool inAngle = Vector3.Angle(new Vector3(owner.transform.forward.x, 0, owner.transform.forward.z),
                                             toPFlat) <= fireAngle;
                bool canSee = !requireLOS || HasLineOfSight();

                if (inRange && inAngle && canSee && fireTimer >= fireCooldown)
                {
                    Fire(toPFlat.normalized);
                    fireTimer = 0f;
                }
                break;
        }

        // Worm�� ���ڸ���(���� ��ũ���� �θ� ���) �� baseDelta �״��
        return baseDelta;
    }

    public void OnHit()
    {
        // �ǰ� �� ��� ����� �ʹٸ� �Ʒ�ó��:
        // SetState(State.Hidden);
        // fireTimer = fireCooldown * 0.5f;
    }

    public void OnDespawn()
    {
        // ����ġ/�ʱ�ȭ
        owner.transform.localPosition = baseLocalPos;
        ToggleRenderers(false);
        ToggleColliders(false);
        state = State.Hidden;
        fireTimer = 0f;
    }

    public void OnTriggerEnter(Collider other)
    {
        // ���� �浹 �ʿ�� ó��(���� ��)
    }

    // ========= ���� ��ƿ =========

    private void CacheComponents()
    {
        rends = owner.GetComponentsInChildren<Renderer>(true);
        cols = owner.GetComponentsInChildren<Collider>(true);
    }

    private void SetState(State next, bool instant = false)
    {
        state = next;

        if (state == State.Hidden)
        {
            // �Ʒ��� ��¦ ������ ����/�ݶ��̴� OFF
            //owner.transform.localPosition = baseLocalPos + new Vector3(0f, -burrowDepth, 0f);
            ToggleRenderers(false);
            ToggleColliders(false);
        }
        else // Grounded
        {
            //owner.transform.localPosition = baseLocalPos;
            ToggleRenderers(true);
            ToggleColliders(true);
            // ���� �� �÷��̾� �� ������ ��� ����(������ ����)
            if (player)
            {
                Vector3 look = player.position - owner.transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.0001f)
                    owner.transform.rotation = Quaternion.LookRotation(look.normalized, Vector3.up);
            }
        }
    }

    private void ToggleRenderers(bool on)
    {
        if (rends == null) return;
        for (int i = 0; i < rends.Length; i++) if (rends[i]) rends[i].enabled = on;
    }
    private void ToggleColliders(bool on)
    {
        if (cols == null) return;
        for (int i = 0; i < cols.Length; i++) if (cols[i]) cols[i].enabled = on;
    }

    private void FaceTowards(Vector3 toFlat, float dt)
    {
        if (toFlat.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(toFlat.normalized, Vector3.up);
        owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, target, dt * 6f);
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = owner.transform.position + Vector3.up * muzzleYOffset;
        Vector3 dir = (player.position - origin);
        float maxD = dir.magnitude;
        if (maxD <= 0.001f) return true;
        dir /= maxD;

        if (Physics.Raycast(origin, dir, out var hit, maxD))
            return hit.collider.CompareTag(ConstData.PlayerTag);

        return true;
    }

    // WormBrain.cs ����
    private void Fire(Vector3 dirToPlayerFlat)
    {
        owner.StartCoroutine(FireCoroutine(dirToPlayerFlat));
    }

    private IEnumerator FireCoroutine(Vector3 dirToPlayerFlat)
    {
        owner.Anim.SetTrigger("doAttack");
        yield return new WaitForSeconds(0.22f);
        // 1) Y�� ����: XZ ��� ���⸸ ��� (yaw��)
        Vector3 baseDir = new Vector3(dirToPlayerFlat.x, 0f, dirToPlayerFlat.z).normalized;
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = new Vector3(owner.transform.forward.x, 0f, owner.transform.forward.z).normalized;

        // 2) 3���� �� ����(��): �߾� 0��, ��/�� ��branchDeg
        const float branchDeg = 15f;     // �¿� ������ ����
        const float jitterDeg = 2f;      // ��ü ������ ��¦ ����(����)
        float baseJitter = Random.Range(-jitterDeg, jitterDeg);

        float[] angles = { 0f, -branchDeg, +branchDeg };

        Vector3 muzzle = owner.transform.position + Vector3.up * muzzleYOffset;

        foreach (float a in angles)
        {
            // yaw�� ȸ��(������ ����)
            Quaternion yawRot = Quaternion.AngleAxis(a + baseJitter, Vector3.up);
            Vector3 fireDir = (yawRot * baseDir).normalized;     // �� Y=0 ������ ��� ����

            var proj = ProjectilePoolManager.Instance.Get();

            // ó������ �߻� ������ �ٶ󺸰�(roll/pitch ���� yaw��)
            proj.transform.rotation = Quaternion.LookRotation(fireDir, Vector3.up);

            // Launch���� ��� ���� ���� �� �������θ� ���ư�
            proj.Launch(muzzle, fireDir, projSpeed, projLife, projDizzyOnHit, parentForScroll);
        }

        // �ʿ��ϸ� ����/���� VFX ���⼭
        // SoundManager.instance.Play_SoundEffect(...);
    }

}
