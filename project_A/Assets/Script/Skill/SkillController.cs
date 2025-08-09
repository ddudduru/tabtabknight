// SkillController.cs
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    private readonly Dictionary<int, CompositeSkill> _skills = new();

    // 현재 실행 중인 스킬
    public CompositeSkill _activeSkill;

    /// <summary>스킬 등록</summary>
    public void RegisterSkill(int level, CompositeSkill skill)
    {
        if (!_skills.ContainsKey(level))
            _skills[level] = skill;
    }

    /// <summary>스킬 사용 가능 여부</summary>
    public bool CanUseSkill(int level, float currentStamina)
    {
        return _skills.TryGetValue(level, out var skill)
            && currentStamina >= skill.Cost;
    }

    /// <summary>
    /// 스킬 사용 시도
    /// 1) 이미 실행 중이면 취소
    /// 2) 스태미나 차감
    /// 3) Activate() 호출
    /// </summary>
    public float UseSkill(int level, float currentStamina)
    {
        if (!_skills.TryGetValue(level, out var skill) || currentStamina < skill.Cost)
            return currentStamina;

        // (1) 진행 중인 스킬이 있으면 강제 취소
        EndActiveSkill();

        // (2) 스태미나 차감
        currentStamina -= skill.Cost;

        // (3) 새 스킬 실행
        _activeSkill = skill;
        skill.Activate();

        return currentStamina;
    }

    /// <summary>
    /// 진행 중인 스킬(지속형 포함)을 즉시 종료합니다.
    /// </summary>
    public void EndActiveSkill()
    {
        if (_activeSkill == null) return;

        // (1) Cancel 호출 → 코루틴 중지
        _activeSkill.ExecutionStrategy.Cancel(_activeSkill.Owner);

        // (2) 지속형이라면 이펙트 정리
        if (_activeSkill.EffectStrategy is IContinuousEffectStrategy ce)
            ce.End(_activeSkill.Owner);

        // (3) Animator 파라미터 리셋
        var anim = _activeSkill.Owner.GetComponent<Animator>();
        if (anim != null)
            foreach (var p in _activeSkill.AnimatorParams)
                p.Reset(anim);

        _activeSkill = null;
    }
}
