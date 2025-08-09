// ITargetingStrategy.cs
using UnityEngine;

public interface ITargetingStrategy
{
    /// <summary>
    /// 스킬 발동 시점에, 스킬이 영향을 줄 위치나 오브젝트 컬렉션을 반환합니다.
    /// 반환값 예: Transform, Vector3, Collider[] 등
    /// </summary>
    object GetTargets(GameObject owner);
}
