// IEffectStrategy.cs
using UnityEngine;

/// <summary>
/// ��ų ����Ʈ�� �����ϰ� �������� �ִ� ���Ҹ� ����.
/// </summary>
public interface IEffectStrategy
{
    /// <param name="owner">
    /// ��ų�� ����ϴ� ������Ʈ(��: �÷��̾�)
    /// </param>
    void Apply(GameObject owner);
}
