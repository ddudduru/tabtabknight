// IContinuousEffectStrategy.cs
using UnityEngine;

/// <summary>
/// ������ ��ų�� ���, ���� Apply()���� VFX�� �����ϰ� �������� �ָ�,
/// ���Ŀ��� �������� �ݺ� ����. ��ų ���� �� End() ȣ��� VFX�� ��ȯ.
/// </summary>
public interface IContinuousEffectStrategy : IEffectStrategy
{
    /// <summary>
    /// ���� ��ų�� ���� �� ȣ��Ǿ�� �ϴ� ���� ����.
    /// ��: ������ VFX GameObject�� Ǯ�� ��ȯ.
    /// </summary>
    void End(GameObject owner);
}