// NearestEnemyDashEffect.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �÷��̾� ���ʿ� �ִ� ���� �� ���� ����� ���� ã�Ƽ� �ش� �������� ���� �̵�(dash)�ϰ� �������� �ִ� ȿ��.
/// IEffectStrategy�� �����Ǿ�, SingleExecution�� ���� �� �ֽ��ϴ�.
/// </summary>
public class NearestEnemyDashEffect : IEffectStrategy
{
    private float damageAmount;
    private float dashDistance;
    private float searchRadius;
    private float searchAngle; // ���� ���� ���� (degrees)
    private EffectPoolKind vfxKind;
    private SoundManager.SoundType soundEffect;
    private GameObject spawnedVFX;

    /// <param name="damageAmount">����� ��</param>
    /// <param name="dashDistance">��� �������� ���� �̵��� �Ÿ�</param>
    /// <param name="searchRadius">Ž�� �ݰ�</param>
    /// <param name="searchAngle">
    ///   �÷��̾� ���� �������� �󸶸�ŭ�� ���� ���� �� ���� ã���� (��: 60����� ���� 60�� ����)
    /// </param>
    /// <param name="vfxKind">����Ʈ Ǯ���� ����� VFX ����</param>
    /// <param name="soundEffect">���� ����Ʈ Ÿ��</param>
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
                // ���� ���� ���� �ִ��� Ȯ�� (searchAngle/2 �̳�)
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
            // �� �������� ���� �̵�
            Vector3 dir = (closestEnemy.transform.position - origin).normalized;
            owner.transform.position += dir * dashDistance;

            // ���� ��� �ִٸ� ������ ó��
            if (closestEnemy.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.HitEnemy();
            }

            // VFX ����
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
