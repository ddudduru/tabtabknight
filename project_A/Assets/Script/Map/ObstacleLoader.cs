// ---------------------------------------------------
// ObstacleLoader.cs
// ---------------------------------------------------
using UnityEngine;

public class ObstacleLoader : MonoBehaviour
{
    [Header("Map JSON (TextAsset)")]
    public TextAsset mapJson;
    [Header("Obstacle Prefabs")]
    public GameObject treePrefab;
    public GameObject logPrefab;
    public GameObject rockPrefab;
    [Header("Settings")]
    public Transform parent;
    public float cellSize = 2f;

    void Start()
    {
        if (mapJson == null) return;

        ObstacleMapData mapData = ObstacleMapData.FromJson(mapJson.text);
        for (int y = 0; y < mapData.height; y++)
        {
            for (int x = 0; x < mapData.width; x++)
            {
                ObstacleType type = mapData.GetCell(x, y);
                GameObject prefab = null;
                switch (type)
                {
                    case ObstacleType.Tree:
                        prefab = treePrefab;
                        break;
                    case ObstacleType.Log:
                        prefab = logPrefab;
                        break;
                    case ObstacleType.Rock:
                        prefab = rockPrefab;
                        break;
                    case ObstacleType.Random:
                        int r = Random.Range(0, 3);
                        prefab = (r == 0 ? treePrefab : (r == 1 ? logPrefab : rockPrefab));
                        break;
                    default:
                        continue;
                }

                Vector3 pos = new Vector3(x * cellSize, 0f, y * cellSize);
                Instantiate(prefab, pos, Quaternion.identity, parent);
            }
        }
    }
}
