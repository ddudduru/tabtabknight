using System.Collections.Generic;
using UnityEngine;

public interface IExecutionStrategy
{
    /// <summary>
    /// ��ų ����
    /// </summary>
    /// <param name="owner">��ų ������</param>
    /// <param name="effect">��ƼŬ ������������ ����</param>
    /// <param name="resetParams">
    ///   ��ų ���� �� ������ AnimatorParameter ���
    /// </param>
    void Execute(
        GameObject owner,
        IEffectStrategy effect,
        List<AnimatorParameter> resetParams
    );

    /// <summary>
    /// ��ų ���� ��� �� ȣ��
    /// </summary>
    void Cancel(GameObject owner);
}
