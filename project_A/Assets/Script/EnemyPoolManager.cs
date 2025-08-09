// ---------------------------------------------------
// EnemyPoolManager.cs
// ---------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

// ��ֹ�/������ Ǯ �Ŵ����� �����ϰ�, ���� Ǯ�� �����մϴ�.
public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject ghostPrefab;      // Ghost ������ (Enemy ������Ʈ ����)
    public GameObject skeletonPrefab;    // Skeleton ������
    public GameObject batPrefab;         // Bat ������
    public GameObject slimePrefab;       // Slime ������

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
    /// pool���� Enemy�� �����ų� ������ ���� Instantiate�Ͽ� ��ȯ�մϴ�.
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
    /// Enemy�� Ǯ�� ��ȯ
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
