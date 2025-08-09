// IContinuousEffectStrategy.cs
using UnityEngine;

/// <summary>
/// 지속형 스킬의 경우, 최초 Apply()에서 VFX를 생성하고 데미지를 주며,
/// 이후에는 데미지만 반복 수행. 스킬 종료 시 End() 호출로 VFX를 반환.
/// </summary>
public interface IContinuousEffectStrategy : IEffectStrategy
{
    /// <summary>
    /// 지속 스킬이 끝날 때 호출되어야 하는 정리 로직.
    /// 예: 생성된 VFX GameObject를 풀로 반환.
    /// </summary>
    void End(GameObject owner);
}