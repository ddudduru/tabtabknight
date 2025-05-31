using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePoolManager : MonoBehaviour
{
    // 1) �̱��� �ν��Ͻ�
    public static ObstaclePoolManager Instance { get; private set; }

    [Header("Ǯ���� ������Ʈ ������")]
    public GameObject treePrefab;   // Tree Ÿ�� Prefab
    public GameObject rockPrefab;   // Rock Ÿ�� Prefab
    public GameObject logPrefab;    // Log Ÿ�� Prefab

    // ���ο��� ������ �����ϴ� Ǯ(Queue)
    private Queue<Obstacls_Control> treePool = new Queue<Obstacls_Control>();
    private Queue<Obstacls_Control> rockPool = new Queue<Obstacls_Control>();
    private Queue<Obstacls_Control> logPool = new Queue<Obstacls_Control>();

    private void Awake()
    {
        // �̱��� �ʱ�ȭ: �̹� �����ϸ� �ı�
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // Ǯ �Ŵ����� �� ��ȯ �ÿ��� �ı����� �ʵ��� �� ���� �ֽ��ϴ�.
        // �ʿ��ϴٸ� �Ʒ� �ּ��� �����ϼ���.
        // DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// ��ֹ� Ǯ���� �ϳ� �����ų�, ������ ���� Instantiate �ؼ� ��ȯ.
    /// </summary>
    /// <param name="type">ObstacleType(Tree, Rock, Log)</param>
    /// <returns>�Ҵ�� Obstacls_Control �ν��Ͻ�</returns>
    public Obstacls_Control GetObstacle(ObstacleType type)
    {
        Obstacls_Control result = null;

        switch (type)
        {
            case ObstacleType.Tree:
                if (treePool.Count > 0)
                {
                    result = treePool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(treePrefab);
                    result = go.GetComponent<Obstacls_Control>();
                }
                break;

            case ObstacleType.Rock:
                if (rockPool.Count > 0)
                {
                    result = rockPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(rockPrefab);
                    result = go.GetComponent<Obstacls_Control>();
                }
                break;

            case ObstacleType.Log:
                if (logPool.Count > 0)
                {
                    result = logPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(logPrefab);
                    result = go.GetComponent<Obstacls_Control>();
                }
                break;
        }

        // Despawn �ݹ��� �����ؼ�, Ǯ�� ������ �� ȣ���ϵ��� ����
        if (result != null)
        {
            result.OnDespawn = null; // Ȥ�� ���� �ݹ��� �������� �� �����Ƿ� �ʱ�ȭ
            result.OnDespawn += () => ReturnToPool(result);
            result.transform.parent = null;
        }
        return result;
    }

    /// <summary>
    /// Ǯ�� ������ �� ȣ��Ǵ� �Լ�
    /// </summary>
    private void ReturnToPool(Obstacls_Control obst)
    {
        obst.gameObject.SetActive(false);
        obst.transform.SetParent(this.transform);
        switch (obst.type)
        {
            case Obstacls_Control.Type.Tree:
                treePool.Enqueue(obst);
                break;
            case Obstacls_Control.Type.Rock:
                rockPool.Enqueue(obst);
                break;
            case Obstacls_Control.Type.Log:
                logPool.Enqueue(obst);
                break;
        }
    }
}
