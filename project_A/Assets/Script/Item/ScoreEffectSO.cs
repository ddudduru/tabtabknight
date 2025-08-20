// ScoreEffectSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffects/Score")]
public class ScoreEffectSO : ItemEffectSO
{
    public int score = 150;
    public override void Apply(Player_Control player)
    {
        player.DisplayScorePopup(score);
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);
    }
}
