// ExplosionEffect.cs
using UnityEngine;

/// <summary>
/// owner ��ġ���� ���� ���� �������� �ְ�, VFX�� �� ���� ����.
/// ���� Apply() ȣ�� �ÿ��� �ƹ� ���� ����, End()���� VFX ��ȯ.
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
        // ���� ȣ��: ���� ������ + VFX ���� + ����
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
        // ���� ȣ�� �ÿ��� �ƹ� ���� ����
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
