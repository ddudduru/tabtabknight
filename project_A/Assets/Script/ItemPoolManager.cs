// ---------------------------------------------------
// ItemPoolManager.cs
// ---------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

// ObstaclePoolManager.cs 구조를 그대로 복제하여 작성했습니다. :contentReference[oaicite:4]{index=4}
public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance { get; private set; }

    [Header("아이템 풀링용 프리팹")]
    public GameObject skillPrefab;   // Skill 아이템 Prefab
    public GameObject forwardPrefab; // Forward 아이템 Prefab
    public GameObject coinPrefab;  // Health 아이템 Prefab

    private Queue<Item> skillPool = new Queue<Item>();
    private Queue<Item> forwardPool = new Queue<Item>();
    private Queue<Item> coinPool = new Queue<Item>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        // 필요 시 씬 전환 간 파괴되지 않도록 설정할 수도 있습니다.
        // DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 풀에서 아이템을 하나 꺼내거나, 없으면 새로 Instantiate 해서 반환합니다.
    /// </summary>
    /// <param name="type">ItemType (Skill, Forward, Health)</param>
    /// <returns>할당된 Item 컴포넌트</returns>
    public Item GetItem(ItemType type)
    {
        Item result = null;

        switch (type)
        {
            case ItemType.Skill:
                if (skillPool.Count > 0)
                {
                    result = skillPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(skillPrefab);
                    result = go.GetComponent<Item>();
                }
                break;

            case ItemType.Forward:
                if (forwardPool.Count > 0)
                {
                    result = forwardPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(forwardPrefab);
                    result = go.GetComponent<Item>();
                }
                break;

            case ItemType.Coin:
                if (coinPool.Count > 0)
                {
                    result = coinPool.Dequeue();
                    result.gameObject.SetActive(true);
                }
                else
                {
                    GameObject go = Instantiate(coinPrefab);
                    result = go.GetComponent<Item>();
                }
                break;

            default:
                return null;
        }

        if (result != null)
        {
            // Despawn 콜백 초기화 및 구독
            result.OnDespawn = null;
            result.OnDespawn += () => ReturnToPool(result);
            result.transform.parent = null;
        }
        return result;
    }

    /// <summary>
    /// 아이템을 풀로 반환합니다.
    /// </summary>
    private void ReturnToPool(Item item)
    {
        item.gameObject.SetActive(false);
        item.transform.SetParent(this.transform);

        switch (item.type)
        {
            case ItemType.Skill:
                skillPool.Enqueue(item);
                break;

            case ItemType.Forward:
                forwardPool.Enqueue(item);
                break;

            case ItemType.Coin:
                coinPool.Enqueue(item);
                break;
        }
    }
}
