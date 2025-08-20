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
        public GameObject prefab;    // 해당 타입 프리팹
        public int prewarm;          // 시작 시 미리 생성
    }

    [Header("아이템 풀링 엔트리(타입-프리팹 매핑)")]
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
            if (!e.prefab) { Debug.LogWarning($"[ItemPool] '{e.type}' 프리팹 비어있음"); continue; }
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
            Debug.LogWarning($"[ItemPool] 미등록 타입 요청: {type}");
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
            Debug.LogError($"[ItemPool] 프리팹 '{prefab.name}'에 Item 컴포넌트 없음");
            return null;
        }
        item.type = type;
        return item;
    }

    private void ReturnToPool(Item item)
    {
        if (!item) return;

        // 비주얼 초기화(자식 OFF), 효과 참조 제거
        item.gameObject.SetActive(false);
        item.transform.SetParent(transform, false);

        if (!_pools.TryGetValue(item.type, out var q))
            _pools[item.type] = q = new Queue<Item>();
        q.Enqueue(item);
    }
}
