using UnityEngine;

public enum EffectPoolKind
{
    DashVFX,
    ExplosionVFX,
    AreaAttackVFX,
    // 필요 시 더 추가
}

/// <summary>
/// 각 EffectPoolKind에 대응되는 파티클/이펙트 프리팹과
/// 초기 풀 크기를 인스펙터에서 설정하기 위한 데이터 객체입니다.
/// </summary>
[System.Serializable]
public class EffectPoolConfig
{
    public EffectPoolKind kind;      // 키 값
    public GameObject prefab;        // 풀링할 파티클·이펙트 프리팹
    public int initialPoolSize = 5;  // 초기 풀 크기
}