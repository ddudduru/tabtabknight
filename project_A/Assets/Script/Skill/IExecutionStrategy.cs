using System.Collections.Generic;
using UnityEngine;

public interface IExecutionStrategy
{
    /// <summary>
    /// 스킬 실행
    /// </summary>
    /// <param name="owner">스킬 소유자</param>
    /// <param name="effect">파티클 생성·데미지 로직</param>
    /// <param name="resetParams">
    ///   스킬 종료 시 리셋할 AnimatorParameter 목록
    /// </param>
    void Execute(
        GameObject owner,
        IEffectStrategy effect,
        List<AnimatorParameter> resetParams
    );

    /// <summary>
    /// 스킬 강제 취소 시 호출
    /// </summary>
    void Cancel(GameObject owner);
}
