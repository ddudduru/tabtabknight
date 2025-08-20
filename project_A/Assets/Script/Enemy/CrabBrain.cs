// CrabBrain.cs
using UnityEngine;

public sealed class CrabBrain : IEnemyBrain
{
    private Enemy owner;
    private Transform player;

    // 설정값
    private float patrolWidth = 12f;
    private float lateralSpeed = 8f;
    private bool bounceOnWalls = true;
    private bool flipLocalScaleX = true;
    private string animFloatMovingSpeed = "movingSpeed";

    // 내부 상태
    private int dir = +1;       // +1: 오른쪽, -1: 왼쪽
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

    // Enemy가 계산한 baseDelta(보통 +Z 전진)에 X축 왕복만 얹어서 반환
    public Vector3 ModifyMove(Vector3 baseDelta, float dt)
    {
        if (lateralSpeed <= 0f || patrolWidth <= 0.01f)
            return baseDelta; // 좌우 이동 비활성

        var tr = owner.transform;

        // 다음 프레임 X 계산
        float xNow = tr.position.x;
        float xNext = xNow + (dir * lateralSpeed * dt);

        // 경계 도달 시 반전 + 경계에 스냅
        if (xNext <= leftX) { xNext = leftX; dir = +1; UpdateFacing(); }
        if (xNext >= rightX) { xNext = rightX; dir = -1; UpdateFacing(); }

        float lateralDelta = xNext - xNow;

        return baseDelta + new Vector3(lateralDelta, 0f, 0f);
    }

    public void OnHit()
    {
        // 간단한 리액션: 방향 반전
        dir *= -1;
        UpdateFacing();
    }

    public void OnDespawn()
    {
        // 상태 초기화(풀 복귀 대비)
        owner = null;
        player = null;
        animator = null;
    }

    public void OnTriggerEnter(Collider other)
    {
        // 좌우 금지/벽 태그에 닿으면 방향 반전
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
