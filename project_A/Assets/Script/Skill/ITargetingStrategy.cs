// ITargetingStrategy.cs
using UnityEngine;

public interface ITargetingStrategy
{
    /// <summary>
    /// ��ų �ߵ� ������, ��ų�� ������ �� ��ġ�� ������Ʈ �÷����� ��ȯ�մϴ�.
    /// ��ȯ�� ��: Transform, Vector3, Collider[] ��
    /// </summary>
    object GetTargets(GameObject owner);
}
