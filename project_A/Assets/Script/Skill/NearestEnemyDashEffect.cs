// NearestEnemyDashEffect.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 앞쪽에 있는 적들 중 가장 가까운 적을 찾아서 해당 방향으로 순간 이동(dash)하고 데미지를 주는 효과.
/// IEffectStrategy로 구현되어, SingleExecution에 사용될 수 있습니다.
/// </summary>
public class NearestEnemyDashEffect : IEffectStrategy
{
    private float damageAmount;
    private float dashDistance;
    private float searchRadius;
    private float searchAngle; // 앞쪽 범위 각도 (degrees)
    private EffectPoolKind vfxKind;
    private SoundManager.SoundType soundEffect;
    private GameObject spawnedVFX;

    /// <param name="damageAmount">대미지 값</param>
    /// <param name="dashDistance">대상 방향으로 순간 이동할 거리</param>
    /// <param name="searchRadius">탐색 반경</param>
    /// <param name="searchAngle">
    ///   플레이어 전방 기준으로 얼마만큼의 각도 범위 내 적을 찾을지 (예: 60도라면 전방 60도 원뿔)
    /// </param>
    /// <param name="vfxKind">이펙트 풀에서 사용할 VFX 종류</param>
    /// <param name="soundEffect">사운드 이펙트 타입</param>
    public NearestEnemyDashEffect(
        float damageAmount,
        float dashDistance,
        float searchRadius,
        float searchAngle,
        EffectPoolKind vfxKind,
        SoundManager.SoundType soundEffect
    )
    {
        this.damageAmount = damageAmount;
        this.dashDistance = dashDistance;
        this.searchRadius = searchRadius;
        this.searchAngle = searchAngle;
        this.vfxKind = vfxKind;
        this.soundEffect = soundEffect;
        this.spawnedVFX = null;
    }

    public void Apply(GameObject owner)
    {
        Vector3 origin = owner.transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, searchRadius);
        GameObject closestEnemy = null;
        float minDistance = float.MaxValue;

        Vector3 forward = owner.transform.forward;

        foreach (var col in hits)
        {
            if (col.CompareTag(ConstData.EnemyTag))
            {
                Vector3 toEnemy = (col.transform.position - origin).normalized;
                // 전방 범위 내에 있는지 확인 (searchAngle/2 이내)
                float angleTo = Vector3.Angle(forward, toEnemy);
                if (angleTo <= searchAngle * 0.5f)
                {
                    float dist = Vector3.Distance(origin, col.transform.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestEnemy = col.gameObject;
                    }
                }
            }
        }

        if (closestEnemy != null)
        {
            // 적 방향으로 순간 이동
            Vector3 dir = (closestEnemy.transform.position - origin).normalized;
            owner.transform.position += dir * dashDistance;

            // 적이 살아 있다면 데미지 처리
            if (closestEnemy.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.HitEnemy();
            }

            // VFX 생성
            if (spawnedVFX == null)
            {
                spawnedVFX = EffectPoolManager.Instance.SpawnEffect(
                    vfxKind,
                    owner.transform,
                    Vector3.zero
                );
                if (soundEffect != SoundManager.SoundType.None)
                    SoundManager.instance.Play_SoundEffect(soundEffect);
            }
        }
    }
}
