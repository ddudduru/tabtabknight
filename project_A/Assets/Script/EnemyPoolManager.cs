// ---------------------------------------------------
// EnemyPoolManager.cs
// ---------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

// 장애물/아이템 풀 매니저와 유사하게, 몬스터 풀을 관리합니다.
public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject ghostPrefab;      // Ghost 프리팹 (Enemy 컴포넌트 포함)
    public GameObject skeletonPrefab;    // Skeleton 프리팹
    public GameObject batPrefab;         // Bat 프리팹
    public GameObject slimePrefab;       // Slime 프리팹

    private Queue<Enemy> ghostPool = new Queue<Enemy>();
    private Queue<Enemy> skeletonPool = new Queue<Enemy>();
    private Queue<Enemy> batPool = new Queue<Enemy>();
    private Queue<Enemy> slimePool = new Queue<Enemy>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// pool에서 Enemy를 꺼내거나 없으면 새로 Instantiate하여 반환합니다.
    /// </summary>
    public Enemy GetEnemy(MonsterType type)
    {
        Enemy result = null;

        switch (type)
        {
            case MonsterType.Ghost:
                if (ghostPool.Count > 0)
                {
                    result = ghostPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(ghostPrefab);
                    result = go.GetComponent<Enemy>();
                }
                break;

            case MonsterType.Skeleton:
                if (skeletonPool.Count > 0)
                {
                    result = skeletonPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(skeletonPrefab);
                    result = go.GetComponent<Enemy>();
                }
                break;

            case MonsterType.Bat:
                if (batPool.Count > 0)
                {
                    result = batPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(batPrefab);
                    result = go.GetComponent<Enemy>();
                }
                break;

            case MonsterType.Slime:
                if (slimePool.Count > 0)
                {
                    result = slimePool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(slimePrefab);
                    result = go.GetComponent<Enemy>();
                }
                break;

            default:
                return null;
        }

        if (result != null)
        {
            result.OnDespawn = null;
            result.OnDespawn += () => ReturnToPool(result);
            result.transform.parent = null;
        }
        return result;
    }

    /// <summary>
    /// Enemy를 풀로 반환
    /// </summary>
    private void ReturnToPool(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        enemy.transform.SetParent(this.transform);

        switch (enemy.monsterType)
        {
            case MonsterType.Ghost:
                ghostPool.Enqueue(enemy);
                break;
            case MonsterType.Skeleton:
                skeletonPool.Enqueue(enemy);
                break;
            case MonsterType.Bat:
                batPool.Enqueue(enemy);
                break;
            case MonsterType.Slime:
                slimePool.Enqueue(enemy);
                break;
        }
    }
}
