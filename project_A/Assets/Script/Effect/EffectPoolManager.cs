// EffectPoolManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EffectPoolKind ������� ��ƼŬ/����Ʈ�� Ǯ�� ����.
/// </summary>
public class EffectPoolManager : MonoBehaviour
{
    public static EffectPoolManager Instance { get; private set; }

    [Header("����� ����Ʈ ������ ���")]
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
    /// ����Ʈ�� Ǯ���� �����ų�, ������ ���� �����ؼ� ��ȯ.
    /// </summary>
    /// <param name="kind">Ǯ Ű</param>
    /// <param name="parent">���� �θ� Transform(������ ���常)</param>
    /// <param name="localPosition">�θ� null����� ��ǥ, �ƴϸ� ���� ��ǥ</param>
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
    /// ��ƼŬ ����� ������ Ǯ�� ��ȯ.
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
