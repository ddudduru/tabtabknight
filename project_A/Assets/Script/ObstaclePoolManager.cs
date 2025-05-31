using System;
using System.Collections.Generic;
using UnityEngine;

public class ObstaclePoolManager : MonoBehaviour
{
    // 1) 싱글턴 인스턴스
    public static ObstaclePoolManager Instance { get; private set; }

    [Header("풀링할 오브젝트 프리팹")]
    public GameObject treePrefab;   // Tree 타입 Prefab
    public GameObject rockPrefab;   // Rock 타입 Prefab
    public GameObject logPrefab;    // Log 타입 Prefab

    // 내부에서 실제로 관리하는 풀(Queue)
    private Queue<Obstacls_Control> treePool = new Queue<Obstacls_Control>();
    private Queue<Obstacls_Control> rockPool = new Queue<Obstacls_Control>();
    private Queue<Obstacls_Control> logPool = new Queue<Obstacls_Control>();

    private void Awake()
    {
        // 싱글턴 초기화: 이미 존재하면 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // 풀 매니저는 씬 전환 시에도 파괴되지 않도록 할 수도 있습니다.
        // 필요하다면 아래 주석을 해제하세요.
        // DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 장애물 풀에서 하나 꺼내거나, 없으면 새로 Instantiate 해서 반환.
    /// </summary>
    /// <param name="type">ObstacleType(Tree, Rock, Log)</param>
    /// <returns>할당된 Obstacls_Control 인스턴스</returns>
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

        // Despawn 콜백을 구독해서, 풀로 복귀할 때 호출하도록 설정
        if (result != null)
        {
            result.OnDespawn = null; // 혹시 이전 콜백이 남아있을 수 있으므로 초기화
            result.OnDespawn += () => ReturnToPool(result);
            result.transform.parent = null;
        }
        return result;
    }

    /// <summary>
    /// 풀로 복귀할 때 호출되는 함수
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
