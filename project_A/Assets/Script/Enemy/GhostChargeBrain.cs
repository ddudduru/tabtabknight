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

    // 회전 속도(도/초)
    float turnSpeedMove = 360f;
    float turnSpeedWindup = 720f;
    float turnSpeedCharge = 1080f;

    // Charge 때 회전 고정 여부
    bool lockRotationOnCharge = true;

    float timer;
    Vector3 chargeDir;
    Quaternion chargeRot; // Charge 동안 유지할 목표 회전

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

                    // 가까워지면 미리 살짝 얼굴을 돌려 둠
                    if (player != null)
                    {
                        Vector3 dir = FlatDir(player.position - owner.transform.position);
                        if (dir.sqrMagnitude > 1e-4f)
                            RotateTowards(dir, dt, turnSpeedMove);
                    }

                    // 감지 → Windup
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

                    // 플레이어 쪽으로 빠르게 정렬
                    if (player != null)
                    {
                        Vector3 dir = FlatDir(player.position - owner.transform.position);
                        if (dir.sqrMagnitude > 1e-4f)
                            RotateTowards(dir, dt, turnSpeedWindup);
                    }

                    timer -= dt;
                    if (timer <= 0f)
                    {
                        // Charge 방향/회전 고정
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

                    // 회전 유지/추적
                    if (lockRotationOnCharge)
                    {
                        // 고정된 목표 회전으로 부드럽게
                        RotateTowards(chargeRot, dt, turnSpeedCharge);
                    }
                    else
                    {
                        // 실시간 플레이어 추적 회전
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

    // Y축 평면 방향만 사용
    private static Vector3 FlatDir(Vector3 v)
    {
        v.y = 0f;
        return v.normalized;
    }

    // dir(벡터)로 회전
    private void RotateTowards(Vector3 dir, float dt, float degPerSec)
    {
        if (owner == null) return;
        Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
        RotateTowards(target, dt, degPerSec);
    }

    // rot(쿼터니언)으로 회전
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
