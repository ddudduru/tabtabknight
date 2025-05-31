using UnityEngine;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float spawnInterval = 5f;

    private ObjectPool<Enemy> enemyPool;
    private int spawnAmount;

    private void Start()
    {
        enemyPool = new ObjectPool<Enemy>(enemyPrefab, poolSize, transform);
        spawnAmount = 5 + 5 * GameManager.instance.gameLevel;
        InvokeRepeating(nameof(SpawnEnemies), spawnInterval, spawnInterval);
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            Vector3 pos = transform.position;
            pos.x = UnityEngine.Random.Range(-9f, 5f);
            pos.z = UnityEngine.Random.Range(-35f, -20f);
            Enemy e = enemyPool.Pop(pos, transform.rotation);
            e.OnReturnToPool += () => enemyPool.Push(e);
        }
    }
}
