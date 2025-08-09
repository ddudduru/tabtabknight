// SingleExecution.cs
using System.Collections.Generic;
using UnityEngine;

public class SingleExecution : IExecutionStrategy
{
    private IEffectStrategy effect;

    public SingleExecution(IEffectStrategy effect) => this.effect = effect;

    public void Execute(
        GameObject owner,
        IEffectStrategy _,           // effectStrategy�� ������� �ʰ� this.effect ���
        List<AnimatorParameter> resetParams
    )
    {
        // 1) ȿ�� ����
        effect.Apply(owner);

        // 2) ��� �Ķ���� ����
        var anim = owner.GetComponent<Animator>();
        if (anim != null && resetParams != null)
        {
            foreach (var p in resetParams)
                p.Reset(anim);
        }
    }

    public void Cancel(GameObject owner)
    {
        // �ܹ����� �� �� �� ��������Ƿ� ���� ó�� �ʿ� ����
    }
}
