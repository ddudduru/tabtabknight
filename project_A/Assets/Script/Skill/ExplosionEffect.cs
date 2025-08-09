// ExplosionEffect.cs
using UnityEngine;

/// <summary>
/// owner 위치에서 순간 폭발 데미지를 주고, VFX를 한 번만 생성.
/// 이후 Apply() 호출 시에는 아무 동작 없이, End()에서 VFX 반환.
/// </summary>
public class ExplosionEffect : IContinuousEffectStrategy
{
    private float damageAmount;
    private float radius;
    private EffectPoolKind vfxKind;
    private SoundManager.SoundType soundEffect;

    private GameObject spawnedVFX;

    public ExplosionEffect(float damageAmount, float radius, EffectPoolKind vfxKind, SoundManager.SoundType soundEffect)
    {
        this.damageAmount = damageAmount;
        this.radius = radius;
        this.vfxKind = vfxKind;
        this.soundEffect = soundEffect;
        spawnedVFX = null;
    }

    public void Apply(GameObject owner)
    {
        // 최초 호출: 폭발 데미지 + VFX 생성 + 사운드
        if (spawnedVFX == null)
        {
            Vector3 center = owner.transform.position;

            Collider[] hits = Physics.OverlapSphere(center, radius);
            foreach (var col in hits)
            {
                if (col.CompareTag(ConstData.EnemyTag) && col.TryGetComponent<Enemy>(out var enemy))
                {
                    GameManager.instance.PointUp((int)damageAmount);
                    enemy.Despawn();
                }
            }

            spawnedVFX = EffectPoolManager.Instance.SpawnEffect(
                vfxKind,
                owner.transform,
                Vector3.zero
            );

            if (soundEffect != SoundManager.SoundType.None)
                SoundManager.instance.Play_SoundEffect(soundEffect);
        }
        // 이후 호출 시에는 아무 동작 없음
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
