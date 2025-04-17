using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StartPlayer : MonoBehaviour
{
    Animator anim;
    public AudioSource as_;
    bool isStart = false;
    public TextMeshProUGUI txt_s;
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (!isStart && Input.GetMouseButtonDown(0))
        {
            isStart = true;
            as_.Play();
            PlayPlayer();
        }
    }
    public void PlayPlayer()
    {
        anim.SetTrigger("GO");
        txt_s.text = "GO";
        Invoke(nameof(GoGameScene), 3f);
    }
    public void GoGameScene()
    {
        LoadScene_Control.LoadScene("GameScene");
    }
}
