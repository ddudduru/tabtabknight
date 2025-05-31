using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ObstacleType, ObstacleMapData는 이전 스크립트를 그대로 사용합니다.

public class MapController : MonoBehaviour
{
    [Header("Map Parts")]
    public Transform[] mapParts;              // 재활용할 맵 파트들
    public float recycleZ = -74f;             // 재활용 시 Z 위치
    public float recycleThreshold = 75f;      // 재활용 체크 기준 위치

    [Header("Pattern Data (JSON)")]
    public TextAsset[] mapPatternJsons;       // 패턴 JSON 파일들
    public TextAsset initPatternJson;

    [Header("Obstacle Prefabs")]
    public GameObject[] treePrefabs;          // 나무 프리팹 리스트
    public GameObject logPrefab;              // 통나무 프리팹
    public GameObject rockPrefab;             // 바위 프리팹

    [Header("Grid Spacing")]
    public float cellSizeX = 3f;              // 셀 X 간격
    public float cellSizeZ = 5f;              // 셀 Z 간격
    public float cellY = 0f;                  // Y 고정값

    private List<ObstacleMapData> patterns = new List<ObstacleMapData>();
    private ObstacleMapData startPattern = null;
    private float mapLengthZ;
    private int nextRecycleIndex = 0;

    private void Awake()
    {
        // JSON → ObstacleMapData 파싱
        foreach (var ta in mapPatternJsons)
        {
            if (ta != null)
            {
                patterns.Add(ObstacleMapData.FromJson(ta.text));
            }
        }

        if (initPatternJson != null)
        {
            startPattern = ObstacleMapData.FromJson(initPatternJson.text);
        }

        // 패턴 높이 * cellSizeZ로 맵 파트 길이 계산
        if (patterns.Count > 0)
            mapLengthZ = patterns[0].height * cellSizeZ;
        else
            mapLengthZ = 0f;
    }


    private void Start()
    {
        InitMapSetting();
    }

    private void InitMapSetting()
    {
        var lastIndex = mapParts.Length - 1;
        SetupPatternForPartContent(lastIndex--);
        SetupPatternForPartContent(lastIndex, startPattern);
    }

    private void Update()
    {
        float deltaZ = GameManager.instance.gameSpd * Time.deltaTime;

        // 모든 맵 파트 Z 방향으로 스크롤
        for (int i = 0; i < mapParts.Length; i++)
        {
            mapParts[i].Translate(Vector3.forward * deltaZ, Space.World);
        }

        // 순차적으로 한 프레임에 하나씩 재활용 체크
        Transform part = mapParts[nextRecycleIndex];
        if (part.position.z >= recycleThreshold)
        {
            // 재활용: 새 Z 위치 계산
            float minZ = float.MaxValue;
            for (int j = 0; j < mapParts.Length; j++)
            {
                if (j == nextRecycleIndex) continue;
                float z = mapParts[j].position.z;
                if (z < minZ) minZ = z;
            }
            float newZ = minZ - mapLengthZ;
            Vector3 newPos = new Vector3(part.position.x, part.position.y, newZ);
            part.position = newPos;

            // 콘텐츠만 리셋
            SetupPatternForPartContent(nextRecycleIndex);
        }
        nextRecycleIndex = (nextRecycleIndex + 1) % mapParts.Length;
    }

    private void SetupPatternForPartContent(int index, ObstacleMapData forceSetMapData = null)
    {
        Transform part = mapParts[index];

        // 자식들 중 Obstacle 태그를 가진 오브젝트 제거
        for (int i = part.childCount - 1; i >= 0; i--)
        {
            Transform ch = part.GetChild(i);
            if (ch.CompareTag("Obstacle"))
            {
                if (ch.TryGetComponent<Obstacls_Control>(out var obstacleControl))
                {
                    obstacleControl.Despawn();
                }
            }
        }

        if (patterns.Count == 0)
            return;

        ObstacleMapData pattern = forceSetMapData;
        // 랜덤 패턴 선택
        if (pattern == null)
        {
            pattern = patterns[Random.Range(0, patterns.Count)];
        }

        if (pattern == null)
        {
            return;
        }

        // “왼쪽 상단” 로컬 기준점 계산
        float halfWidth = (pattern.width - 1) * 0.5f * cellSizeX;
        float halfHeight = (pattern.height - 1) * 0.5f * cellSizeZ;
        Vector3 topLeftLocal = new Vector3(-halfWidth, cellY, +halfHeight);

        // 패턴 그리드 순회
        for (int y = 0; y < pattern.height; y++)
        {
            for (int x = 0; x < pattern.width; x++)
            {
                ObstacleType type = pattern.GetCell(x, y);
                GameObject prefab = null;
                switch (type)
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
                        int r = Random.Range(0, 3);
                        prefab = r switch
                        {
                            0 => treePrefabs[Random.Range(0, treePrefabs.Length)],
                            1 => logPrefab,
                            _ => rockPrefab
                        };
                        break;
                    default:
                        continue;
                }

                // 로컬 오프셋 계산: 오른쪽(x * cellSizeX), 아래(-z 방향으로 y * cellSizeZ)
                Vector3 offset = new Vector3(x * cellSizeX, 0f, -y * cellSizeZ);
                Vector3 localPos = topLeftLocal + offset;

                // 월드 좌표 계산 및 인스턴스
                Vector3 spawnPos = part.TransformPoint(localPos);
                var spawnObject = ObstaclePoolManager.Instance.GetObstacle(type);
                if (spawnObject != null)
                {
                    spawnObject.transform.SetParent(part);
                    spawnObject.transform.position = spawnPos;
                    spawnObject.transform.rotation = Quaternion.identity;
                    spawnObject.tag = "Obstacle";
                }
            }
        }
    }
}
