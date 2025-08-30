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

    // 각 셀마다 장애물 정보
    public List<ObstacleType> obstacleCells;

    // 각 셀마다 아이템 정보
    public List<ItemType> itemCells;

    // 각 셀마다 몬스터 정보
    public List<MonsterType> monsterCells;

    public CombinedMapData(int w, int h)
    {
        width = w;
        height = h;

        obstacleCells = Enumerable.Repeat(ObstacleType.Empty, w * h).ToList();
        itemCells = Enumerable.Repeat(ItemType.None, w * h).ToList();
        monsterCells = Enumerable.Repeat(MonsterType.None, w * h).ToList();
    }

    // 장애물 Getter/Setter
    public ObstacleType GetObstacle(int x, int y) => obstacleCells[y * width + x];
    public void SetObstacle(int x, int y, ObstacleType t) => obstacleCells[y * width + x] = t;

    // 아이템 Getter/Setter
    public ItemType GetItem(int x, int y) => itemCells[y * width + x];
    public void SetItem(int x, int y, ItemType t) => itemCells[y * width + x] = t;

    // 몬스터 Getter/Setter
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
    Empty,   // 배치되지 않음
    Tree,    // 나무
    Log,     // 통나무
    Rock,    // 바위
    FallingRock,
    Random,   // 런타임에 랜덤으로 결정 (Tree, Log, Rock 중 하나)
    End,
}

// ---------------------------------------------------
// ItemType enum
// ---------------------------------------------------
public enum ItemType
{
    None,    // 배치되지 않음
    Skill,   // 스킬 아이템
    Forward, // 포워드 아이템
    Coin,    // 코인 아이템
    Magnet,  // 자석 아이템
    Random   // 런타임에 랜덤으로 결정 (Skill, Forward, Coin 중 하나)
}

// ---------------------------------------------------
// MonsterType enum
// ---------------------------------------------------
public enum MonsterType
{
    None,        // 배치되지 않음
    Ghost,      // 고스트 (돌진형)
    Skeleton,    // 스켈레톤
    Bat,         // 박쥐
    Slime,       // 슬라임 (새로 추가)
    Crab,        // 게 (사이드 무빙형)
    Worm,        // 웜 (원거리 공격형)
    Random       // 런타임에 랜덤으로 결정 (Goblin, Skeleton, Bat, Slime 중 하나)
}
