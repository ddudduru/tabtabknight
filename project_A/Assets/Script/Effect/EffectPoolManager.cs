// EffectPoolManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EffectPoolKind 기반으로 파티클/이펙트를 풀링 관리.
/// </summary>
public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }

    [Header("등록할 이펙트 프리팹 목록")]
    [SerializeField] private List<EffectPoolConfig> poolConfigs;

    private Dictionary<EffectPoolKind, GameObject> prefabMap;
    private Dictionary<EffectPoolKind, Queue<GameObject>> poolMap;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        prefabMap = new Dictionary<EffectPoolKind, GameObject>();
        poolMap = new Dictionary<EffectPoolKind, Queue<GameObject>>();

        foreach (var config in poolConfigs)
        {
            if (config.prefab == null) continue;
            if (prefabMap.ContainsKey(config.kind)) continue;

            prefabMap[config.kind] = config.prefab;
            poolMap[config.kind] = new Queue<GameObject>();

            for (int i = 0; i < config.initialPoolSize; i++)
            {
                var go = InstantiateNewEffect(config.kind);
                poolMap[config.kind].Enqueue(go);
            }
        }
    }

    /// <summary>
    /// 이펙트를 풀에서 꺼내거나, 없으면 새로 생성해서 반환.
    /// </summary>
    /// <param name="kind">풀 키</param>
    /// <param name="parent">붙일 부모 Transform(없으면 월드만)</param>
    /// <param name="localPosition">부모가 null→월드 좌표, 아니면 로컬 좌표</param>
    public GameObject SpawnEffect(EffectPoolKind kind, Transform parent, Vector3 localPosition)
    {
        if (!prefabMap.ContainsKey(kind))
        {
            Debug.LogWarning($"EffectPoolManager: Missing prefab for {kind}");
            return null;
        }

        GameObject instance;
        var queue = poolMap[kind];
        if (queue.Count > 0)
            instance = queue.Dequeue();
        else
            instance = InstantiateNewEffect(kind);

        var pooled = instance.GetComponent<PooledEffectBehavior>();
        pooled.Initialize(kind, parent, localPosition);
        return instance;
    }

    private GameObject InstantiateNewEffect(EffectPoolKind kind)
    {
        var prefab = prefabMap[kind];
        var go = Instantiate(prefab);
        go.SetActive(false);

        if (!go.TryGetComponent<PooledEffectBehavior>(out var behavior))
            behavior = go.AddComponent<PooledEffectBehavior>();

        return go;
    }

    /// <summary>
    /// 파티클 재생이 끝나면 풀로 반환.
    /// </summary>
    public void ReleaseEffect(EffectPoolKind kind, GameObject effectGO)
    {
        if (!poolMap.ContainsKey(kind))
        {
            Destroy(effectGO);
            return;
        }

        effectGO.SetActive(false);
        effectGO.transform.SetParent(null);
        poolMap[kind].Enqueue(effectGO);
    }
}
