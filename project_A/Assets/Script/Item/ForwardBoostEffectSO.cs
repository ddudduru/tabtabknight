// ForwardBoostEffectSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "ItemEffects/ForwardBoost")]
public class ForwardBoostEffectSO : ItemEffectSO
{
    public float duration = 3f;
    public override void Apply(Player_Control player)
    {
        player.StartForward(duration); // �� ���� �޼��� ����
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Item_Get);
    }
}
