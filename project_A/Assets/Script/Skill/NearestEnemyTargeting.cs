// NearestEnemyTargeting.cs
using System.Linq;
using UnityEngine;

public class NearestEnemyTargeting : ITargetingStrategy
{
    private float baseRadius;

    public NearestEnemyTargeting(float baseRadius)
    {
        this.baseRadius = baseRadius;
    }

    public object GetTargets(GameObject owner)
    {
        float radius = baseRadius;
        Collider[] hits = Physics.OverlapSphere(owner.transform.position, radius);
        var enemies = hits
            .Where(c => c.CompareTag(ConstData.EnemyTag))
            .Select(c => c.transform)
            .ToList();

        if (enemies.Count == 0)
            return null;

        return enemies
            .OrderBy(t => Vector3.Distance(owner.transform.position, t.position))
            .First();
    }
}
