// ---------------------------------------------------
// ProjectilePoolManager.cs
// ---------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance { get; private set; }

    [Header("Worm ���� ������")]
    [SerializeField] private WormProjectile projectilePrefab;

    private readonly Queue<WormProjectile> pool = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // �ʿ� ��
    }

    public WormProjectile Get()
    {
        WormProjectile prj = null;
        if (pool.Count > 0)
        {
            prj = pool.Dequeue();
        }
        else
        {
            prj = Instantiate(projectilePrefab);
            prj.Init(ReturnToPool);
        }
        return prj;
    }

    private void ReturnToPool(WormProjectile prj)
    {
        prj.gameObject.SetActive(false);
        prj.transform.SetParent(transform);
        pool.Enqueue(prj);
    }
}
