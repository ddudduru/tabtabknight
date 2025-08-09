// SkillController.cs
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private readonly Dictionary<int, CompositeSkill> _skills = new();

    // ���� ���� ���� ��ų
    public CompositeSkill _activeSkill;

    /// <summary>��ų ���</summary>
    public void RegisterSkill(int level, CompositeSkill skill)
    {
        if (!_skills.ContainsKey(level))
            _skills[level] = skill;
    }

    /// <summary>��ų ��� ���� ����</summary>
    public bool CanUseSkill(int level, float currentStamina)
    {
        return _skills.TryGetValue(level, out var skill)
            && currentStamina >= skill.Cost;
    }

    /// <summary>
    /// ��ų ��� �õ�
    /// 1) �̹� ���� ���̸� ���
    /// 2) ���¹̳� ����
    /// 3) Activate() ȣ��
    /// </summary>
    public float UseSkill(int level, float currentStamina)
    {
        if (!_skills.TryGetValue(level, out var skill) || currentStamina < skill.Cost)
            return currentStamina;

        // (1) ���� ���� ��ų�� ������ ���� ���
        EndActiveSkill();

        // (2) ���¹̳� ����
        currentStamina -= skill.Cost;

        // (3) �� ��ų ����
        _activeSkill = skill;
        skill.Activate();

        return currentStamina;
    }

    /// <summary>
    /// ���� ���� ��ų(������ ����)�� ��� �����մϴ�.
    /// </summary>
    public void EndActiveSkill()
    {
        if (_activeSkill == null) return;

        // (1) Cancel ȣ�� �� �ڷ�ƾ ����
        _activeSkill.ExecutionStrategy.Cancel(_activeSkill.Owner);

        // (2) �������̶�� ����Ʈ ����
        if (_activeSkill.EffectStrategy is IContinuousEffectStrategy ce)
            ce.End(_activeSkill.Owner);

        // (3) Animator �Ķ���� ����
        var anim = _activeSkill.Owner.GetComponent<Animator>();
        if (anim != null)
            foreach (var p in _activeSkill.AnimatorParams)
                p.Reset(anim);

        _activeSkill = null;
    }
}
