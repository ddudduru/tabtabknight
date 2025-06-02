using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UI_Control : MonoBehaviour
{
    static public UI_Control instance;
    public GameObject dizzy_go;
    public TextMeshProUGUI dizzy_text;

    public TextMeshProUGUI score_text;

    public Player_Control player_data;

    public TextMeshProUGUI gameLv_text;
    public TextMeshProUGUI finishScore_text;

    public GameObject skill_go;
    public Image skill_time_img;
    public GameObject Option_panel;
    public GameObject Finish_panel;


    // Update is called once per frame
    private void Awake()
    {
        instance = this;
        gameLv_text.transform.parent.GetComponent<Animator>().SetTrigger("doSlide");
    }
    void Update()
    {
        if (player_data.isDizzy)
        {
            dizzy_go.SetActive(true);
            dizzy_text.text = "" + player_data.dizzy_amount;
        }
        else
        {
            dizzy_go.SetActive(false);
        }
        if (player_data.isImmortal)
        {
            skill_go.SetActive(true);
            skill_time_img.fillAmount = player_data.skill_amount / 4f;
        }
        else
        {
            skill_go.SetActive(false);
        }
        score_text.text = String.Format("{0:#,0}", GameManager.instance.score);

    }
    public void Play_game()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Option_panel.SetActive(false);
        Time.timeScale = 1f;

    }
    public void Finish_game()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Time.timeScale = 0f;
        Finish_panel.SetActive(true);
        finishScore_text.text = score_text.text;


    }
    public void Go_Home()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Time.timeScale = 1f;
        LoadScene_Control.LoadScene("StartScene");
    }
    public void Restart_game()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Time.timeScale = 1f;
        LoadScene_Control.LoadScene("GameScene");
    }
    public void Quit_game()
    {
        Application.Quit();
    }
    public void Click_Option()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Option_panel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void PopUp_Level()
    {
        gameLv_text.transform.parent.GetComponent<Animator>().SetTrigger("doSlide");
        gameLv_text.text = "" + GameManager.instance.gameLevel;
    }

}
