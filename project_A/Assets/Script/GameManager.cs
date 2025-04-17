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
    
    private void Awake()
    {
        instance = this;
        StartCoroutine(Point_UP());
        StartCoroutine(GameLevelUp());
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
    IEnumerator GameLevelUp()
    {
        while (true)
        {
            if (gameLevel >= 7)
            {
                break;
            }
            yield return new WaitForSeconds(30f);
            gameLevel += 1;
            gameSpd += 1f;
            UI_Control.instance.PopUp_Level();
            Map_Control.instance.tree_amount += 5;
        }
    }
    IEnumerator Point_UP()
    {
        while (true)
        {
            score += gameLevel;
            yield return new WaitForSeconds(1f);
        }
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
