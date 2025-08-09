// SkillBase.cs
using UnityEngine;

public abstract class SkillBase
{
    public int Level { get; private set; }
    public float Cost { get; private set; }
    public GameObject Owner { get; private set; }

    protected SkillBase(int level, float cost, GameObject owner)
    {
        Level = level;
        Cost = cost;
        Owner = owner;
    }

    public abstract void Activate();
}
