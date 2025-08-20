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
        // �ʿ��ϸ� �˾��� ���⿡��: player.DisplayScorePopup(��)
    }
}
