using UnityEngine;

public class NearMissBoostSystem : MonoBehaviour
{
    [Header("Refs")]
    public Player_Control player;

    [Header("Near Miss")]
    public float nearMissDistance = 0.6f;
    public float recentTurnWindow = 0.25f;
    public float nearMissCooldown = 0.20f;

    [Header("Speed Model")]
    public float baseWorldSpeed = 2f;
    public float forwardBonusSpeed = 1f;   // applied when player.forwardActive > 0
    public float boostPerNearMiss = 0.5f;
    public float boostMaxAdd = 3f;
    public float boostDecayPerSec = 0.75f;

    private float lastTurnTime = -999f;
    private float lastNearMissTime = -999f;
    private float nearMissBoost = 0f;

    private const int _maxHits = 16;
    private Collider[] _hits;

    // Add these fields on NearMissBoostSystem
    [SerializeField] private LayerMask nearMissMask = ~0; // set to Obstacles|Enemies in Inspector
    [SerializeField] private bool useRecentTurnGate = true;

    private void Awake()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player_Control>();
        }

        _hits = new Collider[_maxHits];

        if (player != null)
        {
            player.OnDirectionToggled += OnTurn;
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnDirectionToggled -= OnTurn;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }
        if (!GameManager.instance.isStart)
        {
            return;
        }

        // 1) decay
        if (nearMissBoost > 0f)
        {
            nearMissBoost -= boostDecayPerSec * Time.fixedDeltaTime;
            if (nearMissBoost < 0f)
            {
                nearMissBoost = 0f;
            }
        }

        // 2) detect near miss only right after a turn
        TryNearMiss();

        // 3) apply world speed (dizzy overrides)
        ApplyWorldSpeed();
    }

    private void OnTurn()
    {
        lastTurnTime = Time.time;
    }

    private bool IsPlayerCollider(Collider c)
    {
        if (c == null) { return false; }

        // same rigidbody as player
        var prb = player.GetComponent<Rigidbody>();
        if (c.attachedRigidbody != null && prb != null && c.attachedRigidbody == prb)
        {
            return true;
        }

        // any child of the player
        if (c.transform != null && c.transform.IsChildOf(player.transform))
        {
            return true;
        }

        return false;
    }

    private bool IsDangerCollider(Collider c)
    {
        if (c == null) { return false; }

        // tag on this transform
        if (c.transform.CompareTag(ConstData.ObstacleTag) || c.transform.CompareTag(ConstData.EnemyTag))
        {
            return true;
        }

        // tag on root (common when child collider is Untagged)
        var root = c.transform.root;
        if (root != null && (root.CompareTag(ConstData.ObstacleTag) || root.CompareTag(ConstData.EnemyTag)))
        {
            return true;
        }

        // fallback: component on parent chain
        if (c.GetComponentInParent<Obstacls_Control>() != null) { return true; }
        if (c.GetComponentInParent<Enemy>() != null) { return true; }

        return false;
    }

    private void TryNearMiss()
    {
        if (useRecentTurnGate && (Time.time - lastTurnTime > recentTurnWindow))
        {
            return;
        }
        if (Time.time - lastNearMissTime < nearMissCooldown)
        {
            return;
        }
        if (player.IsDizzy || player.IsHit)
        {
            return;
        }

        Vector3 p = player.transform.position;

        // slightly larger query radius than threshold to ensure we collect candidates
        float searchRadius = Mathf.Max(nearMissDistance * 1.2f, 0.25f);

        // include triggers explicitly
        int count = Physics.OverlapSphereNonAlloc(
            p,
            searchRadius,
            _hits,
            nearMissMask,
            QueryTriggerInteraction.Collide
        );

        if (count <= 0)
        {
            return;
        }

        float minSqr = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            Collider c = _hits[i];
            if (c == null)
            {
                continue;
            }
            if (IsPlayerCollider(c))
            {
                continue;
            }
            if (!IsDangerCollider(c))
            {
                continue;
            }

            // use ClosestPoint (surface) distance, project to XZ to ignore height offsets
            Vector3 closest = c.ClosestPoint(p);
            closest.y = p.y;
            float sqr = (closest - p).sqrMagnitude;

            if (sqr < minSqr)
            {
                minSqr = sqr;
            }
        }

        if (minSqr == float.MaxValue)
        {
            return;
        }

        // compare squared distances to avoid extra sqrt
        float nearMissSqr = nearMissDistance * nearMissDistance;
        if (minSqr <= nearMissSqr)
        {
            nearMissBoost = Mathf.Min(nearMissBoost + boostPerNearMiss, boostMaxAdd);
            lastNearMissTime = Time.time;

            // optional feedback (sfx, vfx)
            // SoundManager.instance.Play_SoundEffect(...);
        }
    }

    public void ApplyWorldSpeed()
    {
        if (player.IsDizzy)
        {
            MapController.SetWorldSpeed(0f);
            return;
        }

        float speed = baseWorldSpeed;

        if (player.forwardActive > 0f)
        {
            speed += forwardBonusSpeed;
        }

        speed += nearMissBoost;

        MapController.SetWorldSpeed(speed);
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null) { return; }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.transform.position, nearMissDistance);
    }
}


