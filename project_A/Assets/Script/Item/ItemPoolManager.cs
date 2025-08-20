// ItemPoolManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance { get; private set; }

    [Serializable]
    public struct PoolEntry
    {
        public ItemType type;        // Skill, Forward, Coin, Magnet ...
        public GameObject prefab;    // �ش� Ÿ�� ������
        public int prewarm;          // ���� �� �̸� ����
    }

    [Header("������ Ǯ�� ��Ʈ��(Ÿ��-������ ����)")]
    public List<PoolEntry> entries = new();

    private readonly Dictionary<ItemType, Queue<Item>> _pools = new();
    private readonly Dictionary<ItemType, GameObject> _prefabs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildDictionaries();
        PrewarmAll();
    }

    private void BuildDictionaries()
    {
        _pools.Clear(); _prefabs.Clear();
        foreach (var e in entries)
        {
            if (!e.prefab) { Debug.LogWarning($"[ItemPool] '{e.type}' ������ �������"); continue; }
            _prefabs[e.type] = e.prefab;
            if (!_pools.ContainsKey(e.type)) _pools[e.type] = new Queue<Item>();
        }
    }

    private void PrewarmAll()
    {
        foreach (var e in entries)
        {
            if (e.prewarm <= 0) continue;
            if (!_prefabs.TryGetValue(e.type, out var prefab) || !prefab) continue;
            var q = _pools[e.type];
            for (int i = 0; i < e.prewarm; i++)
            {
                var item = InstantiateAndSetup(prefab, e.type);
                item.gameObject.SetActive(false);
                item.transform.SetParent(transform);
                q.Enqueue(item);
            }
        }
    }

    public Item GetItem(ItemType type)
    {
        if (!_prefabs.TryGetValue(type, out var prefab) || !prefab)
        {
            Debug.LogWarning($"[ItemPool] �̵�� Ÿ�� ��û: {type}");
            return null;
        }

        var pool = _pools[type];
        Item result = (pool.Count > 0) ? pool.Dequeue() : InstantiateAndSetup(prefab, type);

        if (result != null)
        {
            result.OnDespawn = null;
            result.OnDespawn += () => ReturnToPool(result);
            result.transform.SetParent(null, true);
            result.gameObject.SetActive(true);
        }
        return result;
    }

    private Item InstantiateAndSetup(GameObject prefab, ItemType type)
    {
        var go = Instantiate(prefab);
        var item = go.GetComponent<Item>();
        if (!item)
        {
            Debug.LogError($"[ItemPool] ������ '{prefab.name}'�� Item ������Ʈ ����");
            return null;
        }
        item.type = type;
        return item;
    }

    private void ReturnToPool(Item item)
    {
        if (!item) return;

        // ���־� �ʱ�ȭ(�ڽ� OFF), ȿ�� ���� ����
        item.gameObject.SetActive(false);
        item.transform.SetParent(transform, false);

        if (!_pools.TryGetValue(item.type, out var q))
            _pools[item.type] = q = new Queue<Item>();
        q.Enqueue(item);
    }
}
