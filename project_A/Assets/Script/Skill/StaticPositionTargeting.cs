// StaticPositionTargeting.cs
using UnityEngine;

public class StaticPositionTargeting : ITargetingStrategy
{
    public object GetTargets(GameObject owner)
    {
        return owner.transform.position; // Vector3
    }
}
