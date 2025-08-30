// ---------------------------------------------------
// CombinedMapData.cs
// ---------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class CombinedMapData
{
    public int width;
    public int height;

    // �� ������ ��ֹ� ����
    public List<ObstacleType> obstacleCells;

    // �� ������ ������ ����
    public List<ItemType> itemCells;

    // �� ������ ���� ����
    public List<MonsterType> monsterCells;

    public CombinedMapData(int w, int h)
    {
        width = w;
        height = h;

        obstacleCells = Enumerable.Repeat(ObstacleType.Empty, w * h).ToList();
        itemCells = Enumerable.Repeat(ItemType.None, w * h).ToList();
        monsterCells = Enumerable.Repeat(MonsterType.None, w * h).ToList();
    }

    // ��ֹ� Getter/Setter
    public ObstacleType GetObstacle(int x, int y) => obstacleCells[y * width + x];
    public void SetObstacle(int x, int y, ObstacleType t) => obstacleCells[y * width + x] = t;

    // ������ Getter/Setter
    public ItemType GetItem(int x, int y) => itemCells[y * width + x];
    public void SetItem(int x, int y, ItemType t) => itemCells[y * width + x] = t;

    // ���� Getter/Setter
    public MonsterType GetMonster(int x, int y) => monsterCells[y * width + x];
    public void SetMonster(int x, int y, MonsterType t) => monsterCells[y * width + x] = t;

    public string ToJson()
    {
        return JsonUtility.ToJson(this, prettyPrint: true);
    }

    public static CombinedMapData FromJson(string json)
    {
        return JsonUtility.FromJson<CombinedMapData>(json);
    }
}

// ---------------------------------------------------
// ObstacleType enum
// ---------------------------------------------------
public enum ObstacleType
{
    Empty,   // ��ġ���� ����
    Tree,    // ����
    Log,     // �볪��
    Rock,    // ����
    FallingRock,
    Random,   // ��Ÿ�ӿ� �������� ���� (Tree, Log, Rock �� �ϳ�)
    End,
}

// ---------------------------------------------------
// ItemType enum
// ---------------------------------------------------
public enum ItemType
{
    None,    // ��ġ���� ����
    Skill,   // ��ų ������
    Forward, // ������ ������
    Coin,    // ���� ������
    Magnet,  // �ڼ� ������
    Random   // ��Ÿ�ӿ� �������� ���� (Skill, Forward, Coin �� �ϳ�)
}

// ---------------------------------------------------
// MonsterType enum
// ---------------------------------------------------
public enum MonsterType
{
    None,        // ��ġ���� ����
    Ghost,      // ��Ʈ (������)
    Skeleton,    // ���̷���
    Bat,         // ����
    Slime,       // ������ (���� �߰�)
    Crab,        // �� (���̵� ������)
    Worm,        // �� (���Ÿ� ������)
    Random       // ��Ÿ�ӿ� �������� ���� (Goblin, Skeleton, Bat, Slime �� �ϳ�)
}
