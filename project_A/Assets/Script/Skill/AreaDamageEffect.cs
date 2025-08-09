// AreaDamageEffect.cs
using UnityEngine;

/// <summary>
/// owner �ֺ� Ư�� �ݰ� �� ������ �������� �ְ�, VFX�� �� ���� ����.
/// ���� Apply() ȣ�� �ÿ��� VFX �ߺ� ���� ���� ������ ������ ����.
/// ��ų�� ������ End()���� VFX�� ��ȯ.
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

        // ���� Apply ��, VFX ���� + ���� ���
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

        // ������ ó�� (�� ȣ�⸶��)
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
