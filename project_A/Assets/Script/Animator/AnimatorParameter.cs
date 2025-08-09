// AnimatorParameter.cs
using UnityEngine;

public enum AnimatorParameterType
{
    Trigger,
    Bool,
    Float,
    Int
}

/// <summary>
/// Animator�� �� ���� ���� �Ķ����(Trigger, Bool, Float, Int)�� �����ϱ� ���� ������ Ŭ����.
/// </summary>
[System.Serializable]
public class AnimatorParameter
{
    public string name;                         // Animator �Ķ���� �̸�
    public AnimatorParameterType parameterType; // �Ķ���� Ÿ��
    public bool boolValue;                      // Bool�� �� ����� ��
    public float floatValue;                    // Float�� �� ����� ��
    public int intValue;                        // Int�� �� ����� ��

    public AnimatorParameter(string name, AnimatorParameterType type)
    {
        this.name = name;
        this.parameterType = type;
        // type�� ���� ���Ǵ� ���� ��ų ��� �� ���� ä�� ����
    }

    /// <summary>
    /// Animator ������Ʈ�� �� �Ķ���͸� ������ �����Ѵ�.
    /// </summary>
    public void ApplyTo(Animator anim)
    {
        switch (parameterType)
        {
            case AnimatorParameterType.Trigger:
                anim.SetTrigger(name);
                break;
            case AnimatorParameterType.Bool:
                anim.SetBool(name, boolValue);
                break;
            case AnimatorParameterType.Float:
                anim.SetFloat(name, floatValue);
                break;
            case AnimatorParameterType.Int:
                anim.SetInteger(name, intValue);
                break;
        }
    }

    /// <summary>
    /// Animator�� ������ �� ���°��� ����
    /// (Trigger�� �ڵ����� �����ų� ResetTrigger�� ȣ������ �ʾƵ� ����)
    /// </summary>
    public void Reset(Animator anim)
    {
        switch (parameterType)
        {
            case AnimatorParameterType.Trigger:
                // Trigger�� �ڵ����� ������Ƿ� ���� ó������ ����
                break;
            case AnimatorParameterType.Bool:
                anim.SetBool(name, false);
                break;
            case AnimatorParameterType.Float:
                anim.SetFloat(name, 0f);
                break;
            case AnimatorParameterType.Int:
                anim.SetInteger(name, 0);
                break;
        }
    }
}
