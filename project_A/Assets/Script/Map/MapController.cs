// ---------------------------------------------------
// MapController.cs (SO-driven Stage integration)
// ---------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController Instance { get; private set; }

    [Header("Map Parts")]
    public Transform[] mapParts;
    public float recycleThreshold = 75f;

    [Header("Stage Boot Data (Optional)")]
    [Tooltip("If assigned, this stage data is applied at Start(). State machine can override later via ApplyStageMap().")]
    public StageMapDataSO defaultStageData;

    [Header("Obstacle Prefabs")]
    public GameObject[] treePrefabs;
    public GameObject logPrefab;
    public GameObject rockPrefab;
    public GameObject fallingRockPrefab;

    [Header("Grid Spacing")]
    public float cellSizeX = 3f;
    public float cellSizeZ = 5f;
    public float cellY = 0f;

    [Header("Progress / Scroll Source")]
    [SerializeField] private LevelProgressManager3D progress;

    [Header("Runtime Control")]
    [SerializeField] public bool isPaused = false;

    // Active stage patterns (SO-driven)
    private readonly List<CombinedMapData> activePatterns = new List<CombinedMapData>();
    private CombinedMapData startPattern = null;

    private float mapLengthZ = 0f;
    private int nextRecycleIndex = 0;

    private Vector3[] originalPartPositions;

    // Global world speed multiplier (0 => stop)
    public static float WorldSpeedMul { get; private set; } = 1f;

    public static void SetWorldSpeed(float worldMul)
    {
        WorldSpeedMul = worldMul;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        // Cache initial positions for clean stage resets
        if (mapParts != null && mapParts.Length > 0)
        {
            originalPartPositions = new Vector3[mapParts.Length];
            for (int i = 0; i < mapParts.Length; i++)
            {
                originalPartPositions[i] = mapParts[i].position;
            }
        }
        else
        {
            originalPartPositions = new Vector3[0];
        }
    }

    private void Start()
    {
        // Boot with default stage data (optional). State machine typically overrides afterward.
        if (defaultStageData != null)
        {
            ApplyStageMap(defaultStageData, resetPositions: true);
        }
        else
        {
            // If no data at boot, you can still initialize empty (won't place contents until a stage is applied)
            if (activePatterns.Count == 0)
            {
                // No-op: wait for ApplyStageMap() from StageState
            }
            else
            {
                InitMapSetting();
            }
        }
    }

    private void Update()
    {
        if (!GameManager.instance.isStart)
        {
            return;
        }

        if (isPaused)
        {
            return;
        }

        if (progress == null)
        {
            return;
        }

        // Drive from a single progress source
        float scrollSpeed = progress.CurrentSpeed;
        Vector3 dir = progress.ScrollDir;
        float delta = scrollSpeed * WorldSpeedMul * Time.deltaTime;

        // Scroll parts
        for (int i = 0; i < mapParts.Length; i++)
        {
            mapParts[i].Translate(dir * delta, Space.World);
        }

        // Recycle one per frame
        Transform part = mapParts[nextRecycleIndex];
        if (Vector3.Dot(part.position, dir) >= recycleThreshold)
        {
            float minProj = float.MaxValue;
            for (int j = 0; j < mapParts.Length; j++)
            {
                if (j == nextRecycleIndex)
                {
                    continue;
                }

                float proj = Vector3.Dot(mapParts[j].position, dir);
                if (proj < minProj)
                {
                    minProj = proj;
                }
            }

            float newProj = minProj - mapLengthZ;

            Vector3 basePos = mapParts[nextRecycleIndex].position;
            Vector3 dirN = dir.normalized;
            float curProj = Vector3.Dot(basePos, dirN);
            float diff = newProj - curProj;
            part.position = basePos + dirN * diff;

            SetupCombinedForPartContent(nextRecycleIndex);
        }

        nextRecycleIndex = (nextRecycleIndex + 1) % mapParts.Length;
    }

    // ------------------------------------------------
    // Public runtime controls
    // ------------------------------------------------
    public void PauseScroll(bool pause)
    {
        isPaused = pause;
    }

    public void ResetMapPositions()
    {
        if (originalPartPositions == null || originalPartPositions.Length != mapParts.Length)
        {
            return;
        }

        for (int i = 0; i < mapParts.Length; i++)
        {
            mapParts[i].position = originalPartPositions[i];
        }
        nextRecycleIndex = 0;
    }

    /// <summary>
    /// Apply a stage's pattern set from StageMapDataSO, rebuild contents, and resume scrolling.
    /// Call this when entering a new StageState (Next / Retry).
    /// </summary>
    public void ApplyStageMap(StageMapDataSO stageData, bool resetPositions = true)
    {
        PauseScroll(true);

        LoadStageData(stageData);

        // Clear all under every part
        for (int idx = 0; idx < mapParts.Length; idx++)
        {
            DespawnAllChildren(mapParts[idx]);
        }

        if (resetPositions)
        {
            ResetMapPositions();
        }

        InitMapSetting();

        PauseScroll(false);
    }

    // ------------------------------------------------
    // Stage data load (SO -> active patterns)
    // ------------------------------------------------
    private void LoadStageData(StageMapDataSO stageData)
    {
        activePatterns.Clear();
        startPattern = null;

        if (stageData != null)
        {
            // Load random pool patterns
            if (stageData.combinedPatternJsons != null)
            {
                foreach (var ta in stageData.combinedPatternJsons)
                {
                    if (ta != null)
                    {
                        activePatterns.Add(CombinedMapData.FromJson(ta.text));
                    }
                }
            }

            // Load start pattern
            if (stageData.initCombinedPatternJson != null)
            {
                startPattern = CombinedMapData.FromJson(stageData.initCombinedPatternJson.text);
            }
        }

        // Recalculate map length from first pattern
        if (activePatterns.Count > 0 && activePatterns[0] != null)
        {
            mapLengthZ = activePatterns[0].height * cellSizeZ;
        }
        else if (startPattern != null)
        {
            mapLengthZ = startPattern.height * cellSizeZ;
        }
        else
        {
            mapLengthZ = 0f;
        }
    }

    // ------------------------------------------------
    // Initial fill for the current stage
    // ------------------------------------------------
    private void InitMapSetting()
    {
        if (mapParts == null || mapParts.Length == 0)
        {
            return;
        }

        int lastIndex = mapParts.Length - 1;

        // Last part: random pool
        SetupCombinedForPartContent(lastIndex);
        lastIndex--;

        // Previous part: explicit start pattern if available
        if (startPattern != null)
        {
            SetupCombinedForPartContent(lastIndex, startPattern);
        }
    }

    // ------------------------------------------------
    // Internals
    // ------------------------------------------------
    private void DespawnAllChildren(Transform part)
    {
        for (int i = part.childCount - 1; i >= 0; i--)
        {
            Transform ch = part.GetChild(i);

            if (ch.CompareTag(ConstData.ObstacleTag))
            {
                if (ch.TryGetComponent<Obstacls_Control>(out var obstacleControl))
                {
                    obstacleControl.Despawn();
                }
            }
            else if (ch.CompareTag(ConstData.ItemTag))
            {
                if (ch.TryGetComponent<Item>(out var itemComp))
                {
                    itemComp.Despawn();
                }
            }
            else if (ch.CompareTag(ConstData.EnemyTag))
            {
                if (ch.TryGetComponent<Enemy>(out var enemyComp))
                {
                    enemyComp.Despawn();
                }
                else
                {
                    Destroy(ch.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Fill a map part with obstacles/items/enemies by a CombinedMapData pattern.
    /// If forcePattern is null, a random pattern from activePatterns is chosen.
    /// </summary>
    private void SetupCombinedForPartContent(int index, CombinedMapData forcePattern = null)
    {
        Transform part = mapParts[index];

        // 1) Clear existing objects on this part
        for (int i = part.childCount - 1; i >= 0; i--)
        {
            Transform ch = part.GetChild(i);

            if (ch.CompareTag(ConstData.ObstacleTag))
            {
                if (ch.TryGetComponent<Obstacls_Control>(out var obstacleControl))
                {
                    obstacleControl.Despawn();
                }
            }
            else if (ch.CompareTag(ConstData.ItemTag))
            {
                if (ch.TryGetComponent<Item>(out var itemComp))
                {
                    itemComp.Despawn();
                }
            }
            else if (ch.CompareTag(ConstData.EnemyTag))
            {
                if (ch.TryGetComponent<Enemy>(out var enemyComp))
                {
                    enemyComp.Despawn();
                }
                else
                {
                    Destroy(ch.gameObject);
                }
            }
        }

        if (forcePattern == null)
        {
            if (activePatterns.Count == 0)
            {
                return;
            }
        }

        CombinedMapData pattern = forcePattern != null
            ? forcePattern
            : activePatterns[Random.Range(0, activePatterns.Count)];

        if (pattern == null)
        {
            return;
        }

        // Grid origin (top-left in local space)
        float halfWidth = (pattern.width - 1) * 0.5f * cellSizeX;
        float halfHeight = (pattern.height - 1) * 0.5f * cellSizeZ;
        Vector3 topLeftLocal = new Vector3(-halfWidth, cellY, +halfHeight);

        // 2) Iterate grid cells and spawn contents
        for (int y = 0; y < pattern.height; y++)
        {
            for (int x = 0; x < pattern.width; x++)
            {
                Vector3 offset = new Vector3(x * cellSizeX, 0f, -y * cellSizeZ);
                Vector3 localPos = topLeftLocal + offset;
                Vector3 spawnPos = part.TransformPoint(localPos);

                // 2-1) Obstacles
                ObstacleType oType = pattern.GetObstacle(x, y);
                if (oType != ObstacleType.Empty)
                {
                    GameObject prefab = null;
                    switch (oType)
                    {
                        case ObstacleType.Tree:
                            {
                                if (treePrefabs != null && treePrefabs.Length > 0)
                                {
                                    prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                                }
                                break;
                            }
                        case ObstacleType.Log:
                            {
                                prefab = logPrefab;
                                break;
                            }
                        case ObstacleType.Rock:
                            {
                                prefab = rockPrefab;
                                break;
                            }
                        case ObstacleType.FallingRock:
                            {
                                prefab = fallingRockPrefab;
                                break;
                            }
                        case ObstacleType.Random:
                            {
                                // Choose among Tree/Log/Rock/FallingRock
                                int rO = Random.Range(0, 4);
                                prefab = (rO == 0)
                                    ? (treePrefabs != null && treePrefabs.Length > 0
                                        ? treePrefabs[Random.Range(0, treePrefabs.Length)]
                                        : null)
                                    : (rO == 1)
                                        ? logPrefab
                                        : (rO == 2)
                                            ? rockPrefab
                                            : fallingRockPrefab;
                                break;
                            }
                    }

                    if (prefab != null)
                    {
                        var spawnObstacle = ObstaclePoolManager.Instance.GetObstacle(oType);
                        if (spawnObstacle != null)
                        {
                            spawnObstacle.transform.SetParent(part);
                            spawnObstacle.transform.position = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);
                            spawnObstacle.transform.rotation = Quaternion.identity;
                            spawnObstacle.tag = ConstData.ObstacleTag;

                            if (oType == ObstacleType.FallingRock)
                            {
                                var fr = spawnObstacle.GetComponent<FallingRockObstacle>();
                                if (fr != null)
                                {
                                    fr.SpawnInit(spawnPos);
                                }
                            }
                        }
                    }
                }

                // 2-2) Items
                ItemType iType = pattern.GetItem(x, y);
                if (iType != ItemType.None)
                {
                    if (iType == ItemType.Random)
                    {
                        // Example: Skill / Forward / Coin / Heart
                        int rI = Random.Range(0, 4);
                        iType = (rI == 0)
                            ? ItemType.Skill
                            : (rI == 1)
                                ? ItemType.Forward
                                : (rI == 2)
                                    ? ItemType.Coin
                                    : ItemType.Heart;
                    }

                    Item spawnItem = ItemPoolManager.Instance.GetItem(iType);
                    if (spawnItem != null)
                    {
                        spawnItem.transform.SetParent(part);
                        spawnItem.transform.position = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);
                        spawnItem.transform.rotation = Quaternion.identity;
                        spawnItem.gameObject.tag = ConstData.ItemTag;
                        spawnItem.type = iType;
                    }
                }

                // 2-3) Enemies
                MonsterType mType = pattern.GetMonster(x, y);
                if (mType != MonsterType.None)
                {
                    if (mType == MonsterType.Random)
                    {
                        // Example: Ghost / Skeleton / Bat / Crab / Slime
                        int rM = Random.Range(0, 5);
                        mType = (rM == 0)
                            ? MonsterType.Ghost
                            : (rM == 1)
                                ? MonsterType.Skeleton
                                : (rM == 2)
                                    ? MonsterType.Bat
                                    : (rM == 3)
                                        ? MonsterType.Crab
                                        : MonsterType.Slime;
                    }

                    Enemy spawnEnemy = EnemyPoolManager.Instance.GetEnemy(mType);
                    if (spawnEnemy != null)
                    {
                        spawnEnemy.transform.SetParent(part);
                        spawnEnemy.transform.position = new Vector3(spawnPos.x, spawnPos.y, spawnPos.z);
                        spawnEnemy.transform.rotation = Quaternion.identity;
                        spawnEnemy.tag = ConstData.EnemyTag;
                        spawnEnemy.monsterType = mType;

                        Transform playerTr = GameObject.FindWithTag(ConstData.PlayerTag)?.transform;
                        spawnEnemy.Initialize(mType, playerTr);
                    }
                }
            }
        }
    }


    public void ClearAllMapContents()
    {
        if (mapParts == null)
        {
            return;
        }
        for (int i = 0; i < mapParts.Length; i++)
        {
            DespawnAllChildren(mapParts[i]);
        }
    }

    public void FullReset(bool clearPatterns)
    {
        // Hard stop scrolling and clear everything under parts
        PauseScroll(true);
        ClearAllMapContents();
        ResetMapPositions();

        if (clearPatterns)
        {
            // If your MapController uses stage-driven lists, clear them here
            // (activePatterns/startPattern/mapLengthZ in the SO-driven version)
            var f = typeof(MapController).GetField("activePatterns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sp = typeof(MapController).GetField("startPattern", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null)
            {
                var list = f.GetValue(this) as System.Collections.IList;
                if (list != null)
                {
                    list.Clear();
                }
            }
            if (sp != null)
            {
                sp.SetValue(this, null);
            }
            mapLengthZ = 0f;
        }
    }

    /* Optional but useful: remove any stray tagged objs not parented under map parts */
    public void PurgeStraySceneObjects()
    {
        string[] tags = new string[]
        {
        ConstData.EnemyTag,
        ConstData.ItemTag,
        ConstData.ObstacleTag
        };

        for (int t = 0; t < tags.Length; t++)
        {
            var arr = GameObject.FindGameObjectsWithTag(tags[t]);
            for (int i = 0; i < arr.Length; i++)
            {
                if (!IsUnderAnyPart(arr[i].transform))
                {
                    // Prefer returning to pool if available; otherwise Destroy
                    var enemy = arr[i].GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.Despawn();
                    }
                    else
                    {
                        var item = arr[i].GetComponent<Item>();
                        if (item != null)
                        {
                            item.Despawn();
                        }
                        else
                        {
                            var obs = arr[i].GetComponent<Obstacls_Control>();
                            if (obs != null)
                            {
                                obs.Despawn();
                            }
                            else
                            {
                                Destroy(arr[i]);
                            }
                        }
                    }
                }
            }
        }
    }

    private bool IsUnderAnyPart(Transform tr)
    {
        if (mapParts == null)
        {
            return false;
        }
        for (int i = 0; i < mapParts.Length; i++)
        {
            if (tr.IsChildOf(mapParts[i]))
            {
                return true;
            }
        }
        return false;
    }
}
