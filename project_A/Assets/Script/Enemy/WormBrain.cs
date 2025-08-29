// ---------------------------------------------------
// WormBrain.cs
// IEnemyBrain 기반: 숨음(Hidden) ↔ 지상(Grounded) 상태 + 원거리 사격
// ---------------------------------------------------
using System.Collections;
using UnityEngine;

public class WormBrain : IEnemyBrain
{
    private enum State { Hidden, Grounded }

    private Enemy owner;
    private Transform player;
    private Transform parentForScroll; // 보통 맵 파트(스크롤 상속)

    // ===== 탐지/공격 파라미터 =====
    [Header("탐지/공격")]
    private float detectRange = 40f;     // 이 이하면 지상으로 올라옴(Grounded)
    private float exitRange = 50f;     // 이 이상 멀어지면 다시 숨어감(Hidden) (히스테리시스)
    private float fireRange = 100f;     // 사격 가능한 거리
    private float fireAngle = 55f;     // 전방 기준 사격 허용 각도
    private bool requireLOS = true;    // 단순 시야(레이) 체크

    private float fireCooldown = 1f;   // 한 발 쏘고 다음 발까지 대기
    private float fireTimer = 0f;

    // ===== 연출/히트박스 =====
    private float burrowDepth = 0.5f;    // 숨을 때 지면 아래로 내리는 높이(연출용)
    private Vector3 baseLocalPos;        // 스폰 기준 로컬 위치(지상 위치)
    private Renderer[] rends;
    private Collider[] cols;

    // ===== 투사체 =====
    private float projSpeed = 10f;
    private float projLife = 3.5f;
    private float projDizzyOnHit = 2.5f; // 플레이어 피격 시 어지럼 증가량
    private float muzzleYOffset = 0.7f; // 총구 높이

    private State state;

    // ========= IEnemyBrain 구현 =========

    public void Setup(Enemy owner, Transform player)
    {
        this.owner = owner;
        this.player = player;

        parentForScroll = owner.transform.parent;

        baseLocalPos = owner.transform.localPosition;
        CacheComponents();

        // 처음엔 숨은 상태로 시작
        SetState(State.Hidden, instant: true);
        fireTimer = 0f;
    }

    public Vector3 ModifyMove(Vector3 baseDelta, float dt)
    {
        baseDelta = Vector3.zero;
        if (player == null) return baseDelta;

        // XZ 평면 거리 / 각도
        Vector3 self = owner.transform.position;
        Vector3 toP = player.position - self;
        Vector3 toPFlat = new Vector3(toP.x, 0f, toP.z);
        float dist = toPFlat.magnitude;

        // 상태 전이
        switch (state)
        {
            case State.Hidden:
                // 플레이어가 탐지 범위 안으로 들어오면 지상으로 등장
                if (dist <= detectRange)
                {
                    SetState(State.Grounded);
                    owner.Anim.SetTrigger("doBreakThrough");
                }
                break;

            case State.Grounded:
                // 너무 멀어지면 다시 숨음
                if (dist >= exitRange)
                {
                    SetState(State.Hidden);
                    owner.Anim.SetTrigger("doDive");
                    break;
                }

                // 플레이어 방향으로 천천히 고개 돌리기(연출)
                FaceTowards(toPFlat, dt);

                // 사격 조건 검사
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

        // Worm은 제자리형(월드 스크롤은 부모가 담당) → baseDelta 그대로
        return baseDelta;
    }

    public void OnHit()
    {
        // 피격 후 잠깐 숨기고 싶다면 아래처럼:
        // SetState(State.Hidden);
        // fireTimer = fireCooldown * 0.5f;
    }

    public void OnDespawn()
    {
        // 원위치/초기화
        owner.transform.localPosition = baseLocalPos;
        ToggleRenderers(false);
        ToggleColliders(false);
        state = State.Hidden;
        fireTimer = 0f;
    }

    public void OnTriggerEnter(Collider other)
    {
        // 근접 충돌 필요시 처리(지뢰 등)
    }

    // ========= 내부 유틸 =========

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
            // 아래로 살짝 묻히고 렌더/콜라이더 OFF
            //owner.transform.localPosition = baseLocalPos + new Vector3(0f, -burrowDepth, 0f);
            ToggleRenderers(false);
            ToggleColliders(false);
        }
        else // Grounded
        {
            //owner.transform.localPosition = baseLocalPos;
            ToggleRenderers(true);
            ToggleColliders(true);
            // 등장 시 플레이어 쪽 보도록 즉시 정렬(가벼운 보정)
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

    // WormBrain.cs 내부
    private void Fire(Vector3 dirToPlayerFlat)
    {
        owner.StartCoroutine(FireCoroutine(dirToPlayerFlat));
    }

    private IEnumerator FireCoroutine(Vector3 dirToPlayerFlat)
    {
        owner.Anim.SetTrigger("doAttack");
        yield return new WaitForSeconds(0.22f);
        // 1) Y축 무시: XZ 평면 방향만 사용 (yaw만)
        Vector3 baseDir = new Vector3(dirToPlayerFlat.x, 0f, dirToPlayerFlat.z).normalized;
        if (baseDir.sqrMagnitude < 0.0001f)
            baseDir = new Vector3(owner.transform.forward.x, 0f, owner.transform.forward.z).normalized;

        // 2) 3갈래 샷 각도(도): 중앙 0°, 좌/우 ±branchDeg
        const float branchDeg = 15f;     // 좌우 벌어짐 각도
        const float jitterDeg = 2f;      // 전체 군집에 살짝 랜덤(선택)
        float baseJitter = Random.Range(-jitterDeg, jitterDeg);

        float[] angles = { 0f, -branchDeg, +branchDeg };

        Vector3 muzzle = owner.transform.position + Vector3.up * muzzleYOffset;

        foreach (float a in angles)
        {
            // yaw만 회전(위쪽축 기준)
            Quaternion yawRot = Quaternion.AngleAxis(a + baseJitter, Vector3.up);
            Vector3 fireDir = (yawRot * baseDir).normalized;     // ★ Y=0 유지된 평면 방향

            var proj = ProjectilePoolManager.Instance.Get();

            // 처음부터 발사 방향을 바라보게(roll/pitch 없이 yaw만)
            proj.transform.rotation = Quaternion.LookRotation(fireDir, Vector3.up);

            // Launch에도 평면 방향 전달 → 수평으로만 날아감
            proj.Launch(muzzle, fireDir, projSpeed, projLife, projDizzyOnHit, parentForScroll);
        }

        // 필요하면 사운드/머즐 VFX 여기서
        // SoundManager.instance.Play_SoundEffect(...);
    }

}
