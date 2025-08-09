// CompositeSkill.cs
using System.Collections.Generic;
using UnityEngine;

public class CompositeSkill : SkillBase
{
    private IExecutionStrategy executionStrategy;
    private IEffectStrategy effectStrategy;
    internal List<AnimatorParameter> animatorParams;

    public IExecutionStrategy ExecutionStrategy => executionStrategy;
    public IEffectStrategy EffectStrategy => effectStrategy;
    public List<AnimatorParameter> AnimatorParams => animatorParams;

    public CompositeSkill(
        int level,
        float cost,
        GameObject owner,
        IExecutionStrategy execution,
        IEffectStrategy effect,
        List<AnimatorParameter> animatorParams = null
    ) : base(level, cost, owner)
    {
        executionStrategy = execution;
        effectStrategy = effect;
        this.animatorParams = animatorParams != null
            ? new List<AnimatorParameter>(animatorParams)
            : new List<AnimatorParameter>();
    }

    public override void Activate()
    {
        // (1) Animator 설정
        var anim = Owner.GetComponent<Animator>();
        if (anim != null)
        {
            foreach (var p in animatorParams)
                p.ApplyTo(anim);
        }

        // (2) 실행
        executionStrategy.Execute(Owner, effectStrategy, animatorParams);
    }
}
