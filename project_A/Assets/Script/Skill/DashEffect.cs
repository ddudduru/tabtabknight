// DashEffect.cs
using UnityEngine;

/// <summary>
/// owner�� �������� ���� �̵���Ű��, VFX�� �� ���� ����.
/// ���� Apply() ȣ�� �ÿ��� �ƹ� ���� ����, End()���� VFX ��ȯ.
/// </summary>
public class DashEffect : IContinuousEffectStrategy
{
    private float distance;
    private EffectPoolKind vfxKind;
    private SoundManager.SoundType soundEffect;

    private GameObject spawnedVFX;

    public DashEffect(float distance, EffectPoolKind vfxKind, SoundManager.SoundType soundEffect)
    {
        this.distance = distance;
        this.vfxKind = vfxKind;
        this.soundEffect = soundEffect;
        spawnedVFX = null;
    }

    public void Apply(GameObject owner)
    {
        // ���� ȣ��: �̵� + VFX ���� + ����
        if (spawnedVFX == null)
        {
            Vector3 dashDir = owner.transform.forward;
            owner.transform.position += dashDir * distance;

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
