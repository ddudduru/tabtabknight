// ---------------------------------------------------
// ObstacleMapData.cs
// ---------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType { Empty, Tree, Log, Rock, Random }

[System.Serializable]
public class ObstacleMapData
{
    public int width;
    public int height;
    public List<ObstacleType> cells = new List<ObstacleType>();

    public ObstacleType GetCell(int x, int y)
    {
        return cells[y * width + x];
    }

    public void SetCell(int x, int y, ObstacleType type)
    {
        cells[y * width + x] = type;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this, true);
    }

    public static ObstacleMapData FromJson(string json)
    {
        return JsonUtility.FromJson<ObstacleMapData>(json);
    }
}