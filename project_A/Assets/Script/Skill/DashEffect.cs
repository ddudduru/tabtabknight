// DashEffect.cs
using UnityEngine;

/// <summary>
/// owner를 전방으로 순간 이동시키고, VFX를 한 번만 생성.
/// 이후 Apply() 호출 시에는 아무 동작 없이, End()에서 VFX 반환.
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
        // 최초 호출: 이동 + VFX 생성 + 사운드
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
