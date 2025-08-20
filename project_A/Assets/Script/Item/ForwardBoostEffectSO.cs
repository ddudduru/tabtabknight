// ForwardBoostEffectSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffects/ForwardBoost")]
public class ForwardBoostEffectSO : ItemEffectSO
{
    public float duration = 3f;
    public override void Apply(Player_Control player)
    {
        player.StartForward(duration); // ↑ 보조 메서드 권장
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);
    }
}
