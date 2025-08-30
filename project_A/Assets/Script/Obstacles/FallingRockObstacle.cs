using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class FallingRockObstacle : MonoBehaviour
{
    public enum State
    {
        Idle,
        Warning,
        Falling,
        Landed
    }

    [Header("Config")]
    [SerializeField] private float triggerDistance = 12f;      // horizontal trigger radius
    [SerializeField] private float warningDuration = 1.2f;     // telegraph time before drop
    [SerializeField] private float dropHeight = 10f;           // start height above ground
    [SerializeField] private float damage = 25f;               // damage on hit while falling
    [SerializeField] private float dizzyGain = 4f;             // extra dizzy on hit
    [SerializeField] private EffectPoolKind warningEffect = EffectPoolKind.WarningCircle;
    [SerializeField] private bool cancelIfLeaves = false;      // cancel warning if player leaves radius

    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    [Header("Fall Tuning")]
    [SerializeField] private float extraGravity = 30f;        // extra downward accel (m/s^2)
    [SerializeField] private float initialDownVelocity = 10f; // kick-off downward velocity at begin
    [SerializeField] private float maxFallSpeed = 80f;        // clamp terminal speed (<=0 to ignore)

    // Ground point is stored in LOCAL space to follow moving parent (map part)
    private Vector3 localGroundPoint;
    private GameObject warnVfx;
    private Coroutine warnRoutine;
    private bool didDamage;
    private State state = State.Idle;

    // Small epsilon for landing detection
    private const float landEps = 0.05f;

    private void Awake()
    {
        if (rb == null) { rb = GetComponent<Rigidbody>(); }
        if (col == null) { col = GetComponent<Collider>(); }
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void OnEnable()
    {
        didDamage = false;
        state = State.Idle;
        warnRoutine = null;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        if (col != null)
        {
            col.isTrigger = false; // we want collision events while falling
            col.enabled = true;
        }

        SetVisible(false);                  // ← 경고 전에는 숨김
    }

    /// <summary>
    /// Initialize with a WORLD ground point. It will be stored as LOCAL to follow parent motion.
    /// </summary>
    public void SpawnInit(Vector3 worldGroundPoint)
    {
        if (transform.parent != null)
        {
            localGroundPoint = transform.parent.InverseTransformPoint(worldGroundPoint);
        }
        else
        {
            localGroundPoint = worldGroundPoint; // no parent: local == world
        }

        didDamage = false;
        state = State.Idle;

        // place rock above current WORLD ground point
        Vector3 wp = WorldGroundPoint;
        transform.position = wp + Vector3.up * dropHeight;

        // cleanup any leftover VFX
        if (warnVfx != null)
        {
            EffectPoolManager.Instance.ReleaseEffect(warningEffect, warnVfx);
            warnVfx = null;
        }

        SetVisible(false);  // ← 경고 동안에도 계속 숨김 (요구사항: 경고 ‘후’에 보이기)
        if (col != null) { col.enabled = false; } // ← 충돌도 비활성
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 WorldGroundPoint
    {
        get
        {
            if (transform.parent != null)
            {
                return transform.parent.TransformPoint(localGroundPoint);
            }
            return localGroundPoint;
        }
    }

    private void Update()
    {
        if (state != State.Idle)
        {
            return;
        }

        var player = Player_Control.Instance;
        if (player == null)
        {
            return;
        }

        // horizontal (XZ) distance without sqrt
        Vector3 wp = WorldGroundPoint;
        Vector3 d = player.transform.position - wp;
        d.y = 0f;
        if (d.sqrMagnitude <= triggerDistance * triggerDistance)
        {
            StartWarnOnce();
        }
    }

    private void FixedUpdate()
    {
        // safety: if there is no ground collider, land when reaching target Y
        if (state == State.Falling)
        {
            // extra gravity
            if (rb != null && extraGravity > 0f)
            {
                rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
            }

            // clamp terminal speed
            if (rb != null && maxFallSpeed > 0f)
            {
                Vector3 v = rb.linearVelocity;
                if (v.y < -maxFallSpeed)
                {
                    v.y = -maxFallSpeed;
                    rb.linearVelocity = v;
                }
            }

            // safety landing snap
            float targetY = WorldGroundPoint.y;
            if (transform.position.y <= targetY + 0.05f)
            {
                Land();
            }
        }
    }

    private void StartWarnOnce()
    {
        if (state != State.Idle) { return; }
        if (warnRoutine != null) { return; }

        state = State.Warning;
        warnRoutine = StartCoroutine(CoWarnAndMaybeDrop());
    }

    private IEnumerator CoWarnAndMaybeDrop()
    {
        Vector3 localPos = localGroundPoint + new Vector3(0f, 0.05f, 0f);
        Transform parent = transform.parent; // map part

        warnVfx = EffectPoolManager.Instance.SpawnEffect(
            warningEffect,
            parent,         // parent to the map part
            localPos        // local position under the part
        );

        float t = 0f;
        while (t < warningDuration)
        {
            if (cancelIfLeaves)
            {
                Vector3 wpFixed = (transform.parent != null)
                    ? transform.parent.TransformPoint(localGroundPoint)
                    : localGroundPoint;

                Vector3 d = Player_Control.Instance.transform.position - wpFixed;
                d.y = 0f;

                if (d.sqrMagnitude > triggerDistance * triggerDistance)
                {
                    if (warnVfx != null)
                    {
                        EffectPoolManager.Instance.ReleaseEffect(warningEffect, warnVfx);
                        warnVfx = null;
                    }
                    state = State.Idle;
                    warnRoutine = null;
                    yield break;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        BeginFall();
        warnRoutine = null;
    }

    private void BeginFall()
    {
        // re-evaluate world ground from the fixed local point
        Vector3 wp = (transform.parent != null)
            ? transform.parent.TransformPoint(localGroundPoint)
            : localGroundPoint;

        transform.position = new Vector3(wp.x, wp.y + dropHeight, wp.z);

        state = State.Falling;

        SetVisible(true);
        if (col != null) { col.enabled = true; }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Falling)
        {
            return;
        }

        // damage player only while falling (once)
        var player = collision.collider.GetComponentInParent<Player_Control>();
        if (player != null && !didDamage)
        {
            didDamage = true;
            player.TakeDamage(damage, dizzyGain);
            // continue to fall and land
        }

        if (IsGroundOrStatic(collision.collider))
        {
            Land();
            // remove telegraph
            if (warnVfx != null)
            {
                EffectPoolManager.Instance.ReleaseEffect(warningEffect, warnVfx);
                warnVfx = null;
            }
        }
    }

    // Optional: if other collider is trigger (e.g., player uses trigger collider)
    private void OnTriggerEnter(Collider other)
    {
        if (state != State.Falling)
        {
            return;
        }

        var player = other.GetComponentInParent<Player_Control>();
        if (player != null && !didDamage)
        {
            didDamage = true;
            player.TakeDamage(damage, dizzyGain);
        }

        if (IsGroundOrStatic(other))
        {
            Land();
        }
    }

    // FallingRockObstacle.cs (필드 섹션에 추가)
    [Header("Visuals")]
    [SerializeField] private Renderer[] renderers; // 비워두면 Awake에서 자동 캐싱

    private void SetVisible(bool v)
    {
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }
        for (int i = 0; i < renderers.Length; ++i)
        {
            renderers[i].enabled = v;
        }
    }


    private bool IsGroundOrStatic(Collider c)
    {
        //if (c.CompareTag(ConstData.ObstacleTag)) { return true; }
        if (c.CompareTag("Ground")) { return true; }
        return false;
    }

    private void Land()
    {
        state = State.Landed;

        // snap to ground Y to avoid sinking/hovering
        Vector3 wp = WorldGroundPoint;
        transform.position = new Vector3(transform.position.x, wp.y, transform.position.z);

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // keep collider solid to block the path
        gameObject.tag = ConstData.ObstacleTag;
    }

    public void Despawn()
    {
        if (warnVfx != null)
        {
            EffectPoolManager.Instance.ReleaseEffect(warningEffect, warnVfx);
            warnVfx = null;
        }

        if (warnRoutine != null)
        {
            StopCoroutine(warnRoutine);
            warnRoutine = null;
        }

        didDamage = false;
        state = State.Idle;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
