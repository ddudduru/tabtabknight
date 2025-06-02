using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{

    public AudioSource bgm_as;
    public AudioSource sound_effect_as;

    public AudioClip swing_sword_ac;
    public AudioClip hit_sword_ac;
    public AudioClip hit_wood_ac;
    public AudioClip get_item_ac;
    public AudioClip hit_rock_ac;
    public AudioClip break_rock_ac;
    public AudioClip click_btn_ac;
    public AudioClip hit_player_ac;
    static public SoundManager instance;

    public AudioMixer mixer;

    public Slider bgm_slider;
    public Slider effect_slider;


    public enum SoundType
    {
        None = 0,
        Sword_Slash = 100, // 칼 휘두르는 소리
        Hit_Player = 1000,  // 플레이어 부딪히는 소리
        Sword_Slash_Hit_Normal = 1001,// 칼 맞은 소리
        Sword_Slash_Hit_Wood = 1002, // 칼로 나무 맞은 소리
        Sword_Slash_Hit_Rock = 1003, // 칼로 돌 때린 소리
        Effect_Item_Get = 2001,// 아이템 획득 소리
        Effect_Rock_Break = 2002, // 돌 깨지는 소리
        Effect_Button_Click = 2003, // 버튼 클릭음
    }


    private void Awake()
    {
        instance = this;
        StartBGM();
    }
    public void StartBGM()
    {
        bgm_as.Play();
    }
    public void EndBGM()
    {
        bgm_as.Stop();
    }
    public void Play_SoundEffect(SoundType type)
    {
        switch (type)
        {
            case SoundType.Sword_Slash: // 칼 휘두르는 소리
                sound_effect_as.PlayOneShot(swing_sword_ac);
                break;
            case SoundType.Sword_Slash_Hit_Normal: // 칼 맞은 소리
                sound_effect_as.PlayOneShot(hit_sword_ac);
                break;
            case SoundType.Sword_Slash_Hit_Wood: // 칼로 나무 맞은 소리
                sound_effect_as.PlayOneShot(hit_wood_ac);
                break;
            case SoundType.Sword_Slash_Hit_Rock: // 칼로 돌 때린 소리
                sound_effect_as.PlayOneShot(hit_rock_ac);
                break;
            case SoundType.Effect_Item_Get: // 아이템 획득 소리
                sound_effect_as.PlayOneShot(get_item_ac);
                break;
            case SoundType.Effect_Rock_Break: // 돌 깨지는 소리
                sound_effect_as.PlayOneShot(break_rock_ac);
                break;
            case SoundType.Effect_Button_Click: // 버튼 클릭음
                sound_effect_as.PlayOneShot(click_btn_ac);
                break;
            case SoundType.Hit_Player: // 플레이어 부딪히는 소리
                sound_effect_as.PlayOneShot(hit_player_ac);
                break;
        }
    }
    public void Update()
    {
        bgm_as.volume = bgm_slider.value;
        sound_effect_as.volume = effect_slider.value;
    }



    // Update is called once per frame

}
