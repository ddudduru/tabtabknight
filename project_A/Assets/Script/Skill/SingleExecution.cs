// SingleExecution.cs
using System.Collections.Generic;
using UnityEngine;

public class SingleExecution : IExecutionStrategy
{
    private IEffectStrategy effect;

    public SingleExecution(IEffectStrategy effect) => this.effect = effect;

    public void Execute(
        GameObject owner,
        IEffectStrategy _,           // effectStrategy를 사용하지 않고 this.effect 사용
        List<AnimatorParameter> resetParams
    )
    {
        // 1) 효과 적용
        effect.Apply(owner);

        // 2) 즉시 파라미터 리셋
        var anim = owner.GetComponent<Animator>();
        if (anim != null && resetParams != null)
        {
            foreach (var p in resetParams)
                p.Reset(anim);
        }
    }

    public void Cancel(GameObject owner)
    {
        // 단발형은 딱 한 번 실행됐으므로 별도 처리 필요 없음
    }
}
