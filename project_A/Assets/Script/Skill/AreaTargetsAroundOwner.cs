// AreaTargetsAroundOwner.cs
using UnityEngine;

public class AreaTargetsAroundOwner : ITargetingStrategy
{
    private float baseRadius;

    public AreaTargetsAroundOwner(float baseRadius)
    {
        this.baseRadius = baseRadius;
    }

    public object GetTargets(GameObject owner)
    {
        float radius = baseRadius;
        return Physics.OverlapSphere(owner.transform.position, radius);
    }
}
