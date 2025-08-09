// IEffectStrategy.cs
using UnityEngine;

/// <summary>
/// 스킬 이펙트를 생성하고 데미지를 주는 역할만 수행.
/// </summary>
public interface IEffectStrategy
{
    /// <param name="owner">
    /// 스킬을 사용하는 오브젝트(예: 플레이어)
    /// </param>
    void Apply(GameObject owner);
}
