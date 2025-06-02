// ---------------------------------------------------
// ItemPoolManager.cs
// ---------------------------------------------------
using System;
using System.Collections.Generic;
using UnityEngine;

// ObstaclePoolManager.cs ������ �״�� �����Ͽ� �ۼ��߽��ϴ�. :contentReference[oaicite:4]{index=4}
public class ItemPoolManager : MonoBehaviour
{
    public static ItemPoolManager Instance { get; private set; }

    [Header("������ Ǯ���� ������")]
    public GameObject skillPrefab;   // Skill ������ Prefab
    public GameObject forwardPrefab; // Forward ������ Prefab
    public GameObject coinPrefab;  // Health ������ Prefab

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
        // �ʿ� �� �� ��ȯ �� �ı����� �ʵ��� ������ ���� �ֽ��ϴ�.
        // DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// Ǯ���� �������� �ϳ� �����ų�, ������ ���� Instantiate �ؼ� ��ȯ�մϴ�.
    /// </summary>
    /// <param name="type">ItemType (Skill, Forward, Health)</param>
    /// <returns>�Ҵ�� Item ������Ʈ</returns>
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
            // Despawn �ݹ� �ʱ�ȭ �� ����
            result.OnDespawn = null;
            result.OnDespawn += () => ReturnToPool(result);
            result.transform.parent = null;
        }
        return result;
    }

    /// <summary>
    /// �������� Ǯ�� ��ȯ�մϴ�.
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
