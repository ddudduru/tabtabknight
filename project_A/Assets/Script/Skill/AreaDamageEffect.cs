// AreaDamageEffect.cs
using UnityEngine;

/// <summary>
/// owner 주변 특정 반경 내 적에게 데미지를 주고, VFX를 한 번만 생성.
/// 이후 Apply() 호출 시에는 VFX 중복 생성 없이 데미지 로직만 수행.
/// 스킬이 끝나면 End()에서 VFX를 반환.
/// </summary>
public class AreaDamageEffect : IContinuousEffectStrategy
{
    private float damageAmount;
    private float radius;
    private EffectPoolKind vfxKind;
    private SoundManager.SoundType soundEffect;

    private GameObject spawnedVFX;

    public AreaDamageEffect(float damageAmount, float radius, EffectPoolKind vfxKind, SoundManager.SoundType soundEffect)
    {
        this.damageAmount = damageAmount;
        this.radius = radius;
        this.vfxKind = vfxKind;
        this.soundEffect = soundEffect;
        spawnedVFX = null;
    }

    public void Apply(GameObject owner)
    {
        Vector3 center = owner.transform.position;

        // 최초 Apply 시, VFX 생성 + 사운드 재생
        if (spawnedVFX == null)
        {
            spawnedVFX = EffectPoolManager.Instance.SpawnEffect(
                vfxKind,
                owner.transform,
                new Vector3(0f, 0.5f, 0f)
            );

            if (soundEffect != SoundManager.SoundType.None)
                SoundManager.instance.Play_SoundEffect(soundEffect);
        }

        // 데미지 처리 (매 호출마다)
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var col in hits)
        {
            if (col.CompareTag(ConstData.EnemyTag) && col.TryGetComponent<Enemy>(out var enemy))
            {
                //GameManager.instance.PointUp((int)damageAmount);
                enemy.HitEnemy();
            }
            else if (col.CompareTag(ConstData.ObstacleTag) && col.TryGetComponent<Obstacls_Control>(out var obstacle))
            {
                obstacle.HitObstacle();
            }
        }
    }

    public void End(GameObject owner)
    {
        if (spawnedVFX != null)
        {
            EffectPoolManager.Instance.ReleaseEffect(vfxKind, spawnedVFX);
            spawnedVFX = null;
        }
    }
}
