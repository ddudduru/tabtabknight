// ---------------------------------------------------
// MapController.cs
// ---------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

// ObstacleType, CombinedMapData, ItemType, MonsterType,
// ObstaclePoolManager, ItemPoolManager, EnemyPoolManager,
// ObstacleControl, Item, Enemy 클래스는 미리 정의되어 있어야 합니다.

public class MapController : MonoBehaviour
{
    [Header("Map Parts")]
    public Transform[] mapParts;              // 재활용할 맵 파트들
    public float recycleThreshold = 75f;      // 재활용 체크 기준 Z 위치

    [Header("Combined Pattern Data (JSON)")]
    public TextAsset[] combinedPatternJsons;  // CombinedMapData 패턴 JSON 파일들
    public TextAsset initCombinedPatternJson; // 첫 CombinedMapData 패턴 JSON

    [Header("Obstacle Prefabs")]
    public GameObject[] treePrefabs;          // 나무 프리팹 리스트
    public GameObject logPrefab;              // 통나무 프리팹
    public GameObject rockPrefab;             // 바위 프리팹

    [Header("Grid Spacing")]
    public float cellSizeX = 3f;
    public float cellSizeZ = 5f;
    public float cellY = 0f;

    // 로드된 Combined 패턴 리스트
    private List<CombinedMapData> combinedPatterns = new List<CombinedMapData>();
    // 시작 패턴
    private CombinedMapData startCombinedPattern = null;

    private float mapLengthZ;
    private int nextRecycleIndex = 0;

    [Header("진행/스크롤 소스")]
    [SerializeField] private LevelProgressManager3D progress; // 드래그 or 자동 탐색

    private void Awake()
    {
        // --- CombinedMapData 패턴 로딩 ---
        foreach (var ta in combinedPatternJsons)
        {
            if (ta != null)
            {
                combinedPatterns.Add(CombinedMapData.FromJson(ta.text));
            }
        }
        if (initCombinedPatternJson != null)
        {
            startCombinedPattern = CombinedMapData.FromJson(initCombinedPatternJson.text);
        }

        // 맵 길이 계산 (height × cellSizeZ)
        if (combinedPatterns.Count > 0)
            mapLengthZ = combinedPatterns[0].height * cellSizeZ;
        else
            mapLengthZ = 0f;
    }

    // 맵 스크롤, 적 이동, 발사체 각각 필요하면 분리 가능. 일단 한 번에.
    public static float WorldSpeedMul { get; private set; } = 1f; // 0이면 '정지'

    public static void SetWorldSpeed(float worldMul)
    {
        WorldSpeedMul = worldMul;
    }

    private void Start()
    {
        InitMapSetting();
    }

    private void InitMapSetting()
    {
        int lastIndex = mapParts.Length - 1;

        // 1) 마지막 파트: 랜덤 패턴으로 장애물+아이템+몬스터 배치
        SetupCombinedForPartContent(lastIndex);
        lastIndex--;

        // 2) 다음 파트: 시작 패턴으로 장애물+아이템+몬스터 배치
        if (startCombinedPattern != null)
            SetupCombinedForPartContent(lastIndex, startCombinedPattern);
    }

    private void Update()
    {
        if (progress == null)
            return;

        // ★ ‘단일 소스’에서 현재 속도/방향을 읽는다
        float scrollSpeed = progress.CurrentSpeed;            // 기절/속업 반영된 속도
        Vector3 dir = progress.ScrollDir;               // 전진축 반대(맵이 올라올 방향)

        float delta = scrollSpeed * WorldSpeedMul * Time.deltaTime;

        // 모든 맵 파트 스크롤
        for (int i = 0; i < mapParts.Length; i++)
            mapParts[i].Translate(dir * delta, Space.World);

        // 이하 재활용/스폰 로직은 동일
        Transform part = mapParts[nextRecycleIndex];
        if (Vector3.Dot(part.position, dir) >= recycleThreshold) // ← 방향성 고려
        {
            // 가장 뒤 파트 찾기
            float minProj = float.MaxValue;
            for (int j = 0; j < mapParts.Length; j++)
            {
                if (j == nextRecycleIndex) continue;
                float proj = Vector3.Dot(mapParts[j].position, dir);
                if (proj < minProj) minProj = proj;
            }
            float newProj = minProj - mapLengthZ;
            // dir 방향으로의 위치를 newProj가 되도록 옮김
            Vector3 basePos = mapParts[nextRecycleIndex].position;
            // dir에 대한 스칼라 보정
            float curProj = Vector3.Dot(basePos, dir.normalized);
            float diff = newProj - curProj;
            part.position = basePos + dir.normalized * diff;

            SetupCombinedForPartContent(nextRecycleIndex);
        }
        nextRecycleIndex = (nextRecycleIndex + 1) % mapParts.Length;
    }

    /// <summary>
    /// 하나의 CombinedMapData 패턴으로 장애물, 아이템, 몬스터를 동시에 배치합니다.
    /// </summary>
    private void SetupCombinedForPartContent(int index, CombinedMapData forcePattern = null)
    {
        Transform part = mapParts[index];

        // 1) 파트 내 기존 오브젝트 전부 Despawn(풀 반환)
        for (int i = part.childCount - 1; i >= 0; i--)
        {
            Transform ch = part.GetChild(i);

            // 장애물
            if (ch.CompareTag(ConstData.ObstacleTag))
            {
                if (ch.TryGetComponent<Obstacls_Control>(out var obstacleControl))
                    obstacleControl.Despawn();
            }
            // 아이템
            else if (ch.CompareTag(ConstData.ItemTag))
            {
                if (ch.TryGetComponent<Item>(out var itemComp))
                    itemComp.Despawn();
            }
            // 몬스터
            else if (ch.CompareTag(ConstData.EnemyTag)) // EnemyTag는 “Enemy”로 정의하세요.
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

        if (combinedPatterns.Count == 0)
            return;

        CombinedMapData pattern = forcePattern;
        if (pattern == null)
        {
            // 랜덤 패턴 선택
            pattern = combinedPatterns[Random.Range(0, combinedPatterns.Count)];
        }
        if (pattern == null)
            return;

        // “왼쪽 상단” 로컬 기준점 계산
        float halfWidth = (pattern.width - 1) * 0.5f * cellSizeX;
        float halfHeight = (pattern.height - 1) * 0.5f * cellSizeZ;
        Vector3 topLeftLocal = new Vector3(-halfWidth, cellY, +halfHeight);

        // 2) 패턴 그리드 순회: 장애물 / 아이템 / 몬스터 순서로 배치
        for (int y = 0; y < pattern.height; y++)
        {
            for (int x = 0; x < pattern.width; x++)
            {
                Vector3 offset = new Vector3(x * cellSizeX, 0f, -y * cellSizeZ);
                Vector3 localPos = topLeftLocal + offset;
                Vector3 spawnPos = part.TransformPoint(localPos);

                // 2-1) 장애물 처리
                ObstacleType oType = pattern.GetObstacle(x, y);
                if (oType != ObstacleType.Empty)
                {
                    GameObject prefab = null;
                    switch (oType)
                    {
                        case ObstacleType.Tree:
                            prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                            break;
                        case ObstacleType.Log:
                            prefab = logPrefab;
                            break;
                        case ObstacleType.Rock:
                            prefab = rockPrefab;
                            break;
                        case ObstacleType.Random:
                            int rO = Random.Range(0, 3);
                            prefab = (rO == 0) ? treePrefabs[Random.Range(0, treePrefabs.Length)]
                                 : (rO == 1) ? logPrefab
                                 : rockPrefab;
                            break;
                    }

                    if (prefab != null)
                    {
                        var spawnObstacle = ObstaclePoolManager.Instance.GetObstacle(oType);
                        if (spawnObstacle != null)
                        {
                            spawnObstacle.transform.SetParent(part);
                            spawnObstacle.transform.position = spawnPos;
                            spawnObstacle.transform.rotation = Quaternion.identity;
                            spawnObstacle.tag = ConstData.ObstacleTag;
                        }
                    }
                }

                // 2-2) 아이템 처리
                ItemType iType = pattern.GetItem(x, y);
                if (iType != ItemType.None)
                {
                    if (iType == ItemType.Random)
                    {
                        int rI = Random.Range(0, 3);
                        iType = (rI == 0) ? ItemType.Skill :
                                (rI == 1) ? ItemType.Forward :
                                            (rI == 2) ? ItemType.Coin :
                                            ItemType.Magnet;
                    }

                    Item spawnItem = ItemPoolManager.Instance.GetItem(iType);
                    if (spawnItem != null)
                    {
                        spawnItem.transform.SetParent(part);
                        spawnItem.transform.position = spawnPos;
                        spawnItem.transform.rotation = Quaternion.identity;
                        spawnItem.gameObject.tag = ConstData.ItemTag;
                        spawnItem.type = iType;
                    }
                }

                // 2-3) 몬스터 처리
                MonsterType mType = pattern.GetMonster(x, y);
                if (mType != MonsterType.None)
                {
                    if (mType == MonsterType.Random)
                    {
                        int rM = Random.Range(0, 5);
                        mType = (rM == 0) ? MonsterType.Ghost :
                                (rM == 1) ? MonsterType.Skeleton :
                                (rM == 2) ? MonsterType.Bat :
                                (rM == 3) ? MonsterType.Crab :
                                            MonsterType.Slime;
                    }

                    Enemy spawnEnemy = EnemyPoolManager.Instance.GetEnemy(mType);
                    if (spawnEnemy != null)
                    {
                        spawnEnemy.transform.SetParent(part);
                        spawnEnemy.transform.position = spawnPos;
                        spawnEnemy.transform.rotation = Quaternion.identity;
                        spawnEnemy.tag = ConstData.EnemyTag;
                        spawnEnemy.monsterType = mType;

                        // 플레이어 Transform 확보(예시)
                        Transform playerTr = GameObject.FindWithTag(ConstData.PlayerTag)?.transform;
                        // 초기화(브레인 생성+세팅)
                        spawnEnemy.Initialize(mType, playerTr);
                    }
                }
            }
        }
    }
}
