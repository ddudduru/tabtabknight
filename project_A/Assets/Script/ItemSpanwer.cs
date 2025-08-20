// ItemSpawner.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnEntry
    {
        public ItemType type;          // ��
        public ItemEffectSO effect;    // ����
        public int visualChildIndex;   // ������ �ڽ� ���־� �ε���
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
            Debug.LogWarning("[ItemSpawner] table �������");
            return;
        }

        int count = 3 + GameManager.instance.gameLevel;
        for (int i = 0; i < count; i++)
        {
            var entry = table[Random.Range(0, table.Count)];

            // �� Ǯ���� Ÿ�Ժ� ������ ��������
            Item item = ItemPoolManager.Instance.GetItem(entry.type);
            if (!item) continue;
            var go = item.gameObject;

            // �� ȿ��/���־� ����
            item.effect = entry.effect;
            ActivateVisualChildOnly(go.transform, entry.visualChildIndex);

            // �� ��ġ ���� ��ġ
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
