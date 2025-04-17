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
    public void Play_SoundEffect(int type)
    {
        switch (type)
        {
            case 0: // 칼 휘두르는 소리
                sound_effect_as.PlayOneShot(swing_sword_ac);
                break;
            case 1: // 칼 맞은 소리
                sound_effect_as.PlayOneShot(hit_sword_ac);
                break;
            case 2: // 칼로 나무 맞은 소리
                sound_effect_as.PlayOneShot(hit_wood_ac);
                break;
            case 3: // 아이템 획득 소리
                sound_effect_as.PlayOneShot(get_item_ac);
                break;
            case 4: // 칼로 돌 때린 소리
                sound_effect_as.PlayOneShot(hit_rock_ac);
                break;
            case 5: // 돌 깨지는 소리
                sound_effect_as.PlayOneShot(break_rock_ac);
                break;
            case 6: // 버튼 클릭음
                sound_effect_as.PlayOneShot(click_btn_ac);
                break;
            case 7: // 플레이어 부딪히는 소리
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
