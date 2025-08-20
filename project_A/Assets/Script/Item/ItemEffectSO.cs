// ItemEffectSO.cs
using UnityEngine;

public abstract class ItemEffectSO : ScriptableObject
{
    public abstract void Apply(Player_Control player);
}
