using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffects/HeartHealEffect")]
public class HeartHealEffectSO : ItemEffectSO
{
    public float healAmount = 30f;       // if percent=false: absolute HP
    public bool percent = false;         // true => % of max HP
    public bool showPopup = true;        // optional simple popup using existing method

    public override void Apply(Player_Control player)
    {
        if (player == null)
        {
            return;
        }

        // 1) heal
        player.Heal(healAmount, percent);

        // 2) sfx
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);

        // 3) simple popup (reuse existing score popup)
        if (showPopup)
        {
            // show "+30" or "+15%" text using existing popup (int only, so clamp)
            int shown = Mathf.CeilToInt(percent ? healAmount : healAmount);
            player.DisplayScorePopup(shown); // if you prefer "+30", tweak DisplayScorePopup to prefix '+'
        }
    }
}
