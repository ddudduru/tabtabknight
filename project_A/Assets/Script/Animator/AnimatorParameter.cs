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
/// Animator에 한 번에 여러 파라미터(Trigger, Bool, Float, Int)를 설정하기 위한 데이터 클래스.
/// </summary>
[System.Serializable]
public class AnimatorParameter
{
    public string name;                         // Animator 파라미터 이름
    public AnimatorParameterType parameterType; // 파라미터 타입
    public bool boolValue;                      // Bool일 때 사용할 값
    public float floatValue;                    // Float일 때 사용할 값
    public int intValue;                        // Int일 때 사용할 값

    public AnimatorParameter(string name, AnimatorParameterType type)
    {
        this.name = name;
        this.parameterType = type;
        // type에 따라 사용되는 값은 스킬 등록 시 직접 채워 넣음
    }

    /// <summary>
    /// Animator 컴포넌트에 이 파라미터를 실제로 설정한다.
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
    /// Animator에 “종료 시 리셋값” 적용
    /// (Trigger는 자동으로 꺼지거나 ResetTrigger를 호출하지 않아도 무방)
    /// </summary>
    public void Reset(Animator anim)
    {
        switch (parameterType)
        {
            case AnimatorParameterType.Trigger:
                // Trigger는 자동으로 사라지므로 별도 처리하지 않음
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
