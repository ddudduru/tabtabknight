// ItemSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnEntry
    {
        public ItemType type;          // 라벨
        public ItemEffectSO effect;    // 동작
        public int visualChildIndex;   // 프리팹 자식 비주얼 인덱스
    }

    [Header("Spawn Table")]
    public List<SpawnEntry> table = new();

    [Header("Spawn Rule")]
    public bool autoCycle = true;
    public float cycleSeconds = 7.5f;
    public Vector2 randX = new Vector2(-9f, 5f);
    public Vector2 randZ = new Vector2(-45f, -20f);

    private void Start()
    {
        if (autoCycle) StartCoroutine(ItemSpawnCycle());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) SpawnWave();
    }

    IEnumerator ItemSpawnCycle()
    {
        while (true)
        {
            SpawnWave();
            yield return new WaitForSeconds(cycleSeconds);
        }
    }

    void SpawnWave()
    {
        if (table == null || table.Count == 0)
        {
            Debug.LogWarning("[ItemSpawner] table 비어있음");
            return;
        }

        int count = 3 + GameManager.instance.gameLevel;
        for (int i = 0; i < count; i++)
        {
            var entry = table[Random.Range(0, table.Count)];

            // ▶ 풀에서 타입별 아이템 꺼내오기
            Item item = ItemPoolManager.Instance.GetItem(entry.type);
            if (!item) continue;
            var go = item.gameObject;

            // ▶ 효과/비주얼 세팅
            item.effect = entry.effect;
            ActivateVisualChildOnly(go.transform, entry.visualChildIndex);

            // ▶ 위치 랜덤 배치
            float x = Random.Range(randX.x, randX.y);
            float z = Random.Range(randZ.x, randZ.y);
            go.transform.position = new Vector3(x, go.transform.position.y, z);
        }
    }

    void ActivateVisualChildOnly(Transform root, int onIndex)
    {
        int childCount = root.childCount;
        for (int c = 0; c < childCount; c++)
            root.GetChild(c).gameObject.SetActive(c == onIndex);
    }
}
