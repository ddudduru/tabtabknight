using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager instance;

    public float gameSpd;
    public float time;
    int m, sec;
    public float score;

    public int gameLevel = 1;

    public bool isStart = false;

    private void Awake()
    {
        instance = this;
        gameSpd = 5f;
    }
    private void FixedUpdate()
    {
        time += Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        m = (int)(time / 60);
        sec = (int)(time % 60);
    }

    public void PointUp(int amount)
    {
        score += amount;
    }
    public void ReStart()
    {
        SceneManager.LoadScene("GameScene");
    }
}
