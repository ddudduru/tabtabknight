// FollowExecution.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowExecution : IExecutionStrategy
{
    private IContinuousEffectStrategy continuousEffect;
    private float duration;
    private float interval;
    private Coroutine routine;

    public FollowExecution(IContinuousEffectStrategy continuousEffect, float duration, float interval)
    {
        this.continuousEffect = continuousEffect;
        this.duration = duration;
        this.interval = interval;
    }

    public void Execute(
        GameObject owner,
        IEffectStrategy _,
        List<AnimatorParameter> resetParams
    )
    {
        routine = owner.GetComponent<MonoBehaviour>()
                       .StartCoroutine(PerformFollow(owner, continuousEffect, resetParams));
    }

    public void Cancel(GameObject owner)
    {
        if (routine != null)
        {
            owner.GetComponent<MonoBehaviour>().StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator PerformFollow(
        GameObject owner,
        IContinuousEffectStrategy effect,
        List<AnimatorParameter> resetParams
    )
    {
        float elapsed = 0f;
        bool first = true;

        while (elapsed < duration)
        {
            if (first)
            {
                effect.Apply(owner);
                first = false;
            }
            else
            {
                effect.Apply(owner);
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        effect.End(owner);

        var anim = owner.GetComponent<Animator>();
        if (anim != null && resetParams != null)
        {
            foreach (var p in resetParams)
                p.Reset(anim);
        }
    }
}
