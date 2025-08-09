using UnityEngine;

public class GhostChargeBrain : IEnemyBrain
{
    enum State { Move, Windup, Charge, Recover }
    Enemy owner; Transform player;
    State state = State.Move;

    // ---- Tuning ----
    float detectRange = 15f;
    float windupTime = 0.5f;
    float chargeSpeed = 13f;
    float chargeTime = 3f;
    float recoverTime = 0.2f;

    // ȸ�� �ӵ�(��/��)
    float turnSpeedMove = 360f;
    float turnSpeedWindup = 720f;
    float turnSpeedCharge = 1080f;

    // Charge �� ȸ�� ���� ����
    bool lockRotationOnCharge = true;

    float timer;
    Vector3 chargeDir;
    Quaternion chargeRot; // Charge ���� ������ ��ǥ ȸ��

    public void Setup(Enemy owner, Transform player)
    {
        this.owner = owner;
        this.player = player;
        state = State.Move;
        timer = 0f;
    }

    public Vector3 ModifyMove(Vector3 baseDelta, float dt)
    {
        switch (state)
        {
            case State.Move:
                {
                    if (owner != null && owner.Anim != null)
                    {
                        owner.Anim.SetBool("isStop", false);
                        owner.Anim.SetBool("isAttack", false);
                    }

                    // ��������� �̸� ��¦ ���� ���� ��
                    if (player != null)
                    {
                        Vector3 dir = FlatDir(player.position - owner.transform.position);
                        if (dir.sqrMagnitude > 1e-4f)
                            RotateTowards(dir, dt, turnSpeedMove);
                    }

                    // ���� �� Windup
                    if (player != null && Vector3.Distance(owner.transform.position, player.position) <= detectRange)
                    {
                        state = State.Windup;
                        timer = windupTime;
                    }
                    return baseDelta;
                }

            case State.Windup:
                {
                    if (owner != null && owner.Anim != null)
                    {
                        owner.Anim.SetBool("isStop", true);
                        owner.Anim.SetBool("isAttack", false);
                    }

                    // �÷��̾� ������ ������ ����
                    if (player != null)
                    {
                        Vector3 dir = FlatDir(player.position - owner.transform.position);
                        if (dir.sqrMagnitude > 1e-4f)
                            RotateTowards(dir, dt, turnSpeedWindup);
                    }

                    timer -= dt;
                    if (timer <= 0f)
                    {
                        // Charge ����/ȸ�� ����
                        chargeDir = (player != null)
                            ? FlatDir(player.position - owner.transform.position)
                            : owner.transform.forward;

                        if (chargeDir.sqrMagnitude < 1e-4f)
                            chargeDir = owner.transform.forward;

                        chargeRot = Quaternion.LookRotation(chargeDir, Vector3.up);

                        state = State.Charge;
                        timer = chargeTime;
                    }
                    return Vector3.zero;
                }

            case State.Charge:
                {
                    if (owner != null && owner.Anim != null)
                    {
                        owner.Anim.SetBool("isStop", false);
                        owner.Anim.SetBool("isAttack", true);
                    }

                    // ȸ�� ����/����
                    if (lockRotationOnCharge)
                    {
                        // ������ ��ǥ ȸ������ �ε巴��
                        RotateTowards(chargeRot, dt, turnSpeedCharge);
                    }
                    else
                    {
                        // �ǽð� �÷��̾� ���� ȸ��
                        if (player != null)
                        {
                            Vector3 dir = FlatDir(player.position - owner.transform.position);
                            if (dir.sqrMagnitude > 1e-4f)
                            {
                                chargeDir = dir;
                                RotateTowards(dir, dt, turnSpeedCharge);
                            }
                        }
                    }

                    timer -= dt;
                    Vector3 chargeDelta = chargeDir * chargeSpeed * dt;
                    if (timer <= 0f)
                    {
                        state = State.Recover;
                        timer = recoverTime;
                    }
                    return baseDelta + chargeDelta;
                }

            case State.Recover:
                {
                    timer -= dt;
                    if (timer <= 0f) state = State.Move;
                    return baseDelta;
                }
        }

        // fallback
        return baseDelta;
    }

    public void OnHit() { }
    public void OnDespawn() { timer = 0f; state = State.Move; }

    public void OnTriggerEnter(Collider other)
    {
        if (state != State.Charge) return;
        if (!other.CompareTag(ConstData.PlayerTag)) return;

        Vector3 hitPoint = other.ClosestPoint(owner.transform.position);
        Vector3 hitDir = (other.transform.position - owner.transform.position).normalized;

        Player_Control.Instance.HitPlayer(1f);
    }


    // --- Helpers ---

    // Y�� ��� ���⸸ ���
    private static Vector3 FlatDir(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    // dir(����)�� ȸ��
    private void RotateTowards(Vector3 dir, float dt, float degPerSec)
    {
        if (owner == null) return;
        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        RotateTowards(target, dt, degPerSec);
    }

    // rot(���ʹϾ�)���� ȸ��
    private void RotateTowards(Quaternion target, float dt, float degPerSec)
    {
        if (owner == null) return;
        owner.transform.rotation = Quaternion.RotateTowards(
            owner.transform.rotation,
            target,
            degPerSec * dt
        );
    }
}
