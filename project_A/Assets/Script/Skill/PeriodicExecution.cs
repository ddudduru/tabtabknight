// PeriodicExecution.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeriodicExecution : IExecutionStrategy
{
    private IContinuousEffectStrategy continuousEffect;
    private float duration;
    private float interval;
    private Coroutine routine;

    public PeriodicExecution(IContinuousEffectStrategy continuousEffect, float duration, float interval)
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
                       .StartCoroutine(Perform(owner, continuousEffect, resetParams));
    }

    public void Cancel(GameObject owner)
    {
        if (routine != null)
        {
            owner.GetComponent<MonoBehaviour>().StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator Perform(
        GameObject owner,
        IContinuousEffectStrategy effect,
        List<AnimatorParameter> resetParams
    )
    {
        float elapsed = 0f;
        bool first = true;
        owner.TryGetComponent<Player_Control>(out var player);

        while (elapsed < duration)
        {
            UI_Control.instance.UpdateSkillTime(duration - elapsed, duration);
            if (player.IsDizzy)
            {
                break;
            }

            if (first)
            {
                effect.Apply(owner);    // ����: VFX ���� + ������
                first = false;
            }
            else
            {
                effect.Apply(owner);    // ����: ��������
            }

            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // ���� ���� Ŭ����
        effect.End(owner);
        UI_Control.instance.UpdateSkillTime(0f, duration);

        // �Ķ���� ����
        var anim = owner.GetComponent<Animator>();
        if (anim != null && resetParams != null)
        {
            foreach (var p in resetParams)
                p.Reset(anim);
        }
    }
}
