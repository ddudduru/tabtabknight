// StaminaRestoreEffectSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffects/StaminaRestore")]
public class StaminaRestoreEffectSO : ItemEffectSO
{
    public float amount = 50f;
    public override void Apply(Player_Control player)
    {
        player.RecoverStamina(amount);
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);
        // 필요하면 팝업도 여기에서: player.DisplayScorePopup(…)
    }
}
