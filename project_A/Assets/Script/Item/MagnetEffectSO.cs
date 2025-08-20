// MagnetEffectSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffects/Magnet")]
public class MagnetEffectSO : ItemEffectSO
{
    public float duration = 5f;
    public float interval = 0.02f;
    public float radius = 6f;
    public float pullSpeed = 12f;
    public float yOffset = 0.25f;
    public EffectPoolKind vfxKind = EffectPoolKind.MagnetAuraVFX;

    public override void Apply(Player_Control player)
    {
        var eff = new MagnetContinuousEffect(radius, pullSpeed, yOffset, vfxKind);
        var exec = new PeriodicExecution(eff, duration, interval);
        exec.Execute(player.gameObject, eff, null); // 内风凭栏肺 平 角青
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);
    }
}


public class MagnetContinuousEffect : IContinuousEffectStrategy
{
    private readonly float radius, pullSpeed, yOffset;
    private readonly EffectPoolKind vfxKind;
    private GameObject vfx;
    private static readonly Collider[] buf = new Collider[192];

    public MagnetContinuousEffect(float r, float s, float y, EffectPoolKind vfx)
    { radius = r; pullSpeed = s; yOffset = y; vfxKind = vfx; }

    public void Apply(GameObject owner)
    {
        if (vfx == null && vfxKind != EffectPoolKind.None)
            vfx = EffectPoolManager.Instance.SpawnEffect(vfxKind, owner.transform, Vector3.zero);

        int n = Physics.OverlapSphereNonAlloc(owner.transform.position, radius, buf, ~0, QueryTriggerInteraction.Collide);
        Vector3 target = owner.transform.position + Vector3.up * yOffset;
        float step = pullSpeed * Time.deltaTime;

        for (int i = 0; i < n; i++)
        {
            var c = buf[i];
            if (!c || !c.CompareTag(ConstData.ItemTag)) continue;
            c.transform.position = Vector3.MoveTowards(c.transform.position, target, step);
        }
    }

    public void End(GameObject owner)
    {
        if (vfx != null) { EffectPoolManager.Instance.ReleaseEffect(vfxKind, vfx); vfx = null; }
    }
}
