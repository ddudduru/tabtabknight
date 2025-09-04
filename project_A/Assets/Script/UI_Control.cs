// UI_Control.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Control : MonoBehaviour
{
    public static UI_Control instance;

    [Header("Dizzy UI")]
    [SerializeField] private GameObject dizzyPanel;
    [SerializeField] private TextMeshProUGUI dizzyText;

    [Header("Dizzy UI")]
    [SerializeField] private GameObject HpPanel;
    [SerializeField] private Image hpFrontImage;

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Level UI")]
    [SerializeField] private TextMeshProUGUI gameLevelText;
    [SerializeField] private TextMeshProUGUI finishScoreText;

    [Header("Skill UI")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Image skillTimeImage;

    [Header("Panels")]
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private GameObject finishPanel;

    [Header("Stamina UI")]
    [SerializeField] private Image staminaImage;

    [Tooltip("fillAmount 보간 속도(클수록 더 빨리 따라감)")]
    [SerializeField] private float lerpSpeed = 8f;

    public float targetFill = 0f;   // 외부에서 주입되는 목표 값
    private bool gaugeVisible = false;


    private void Awake()
    {
        instance = this;

        skillPanel.SetActive(false);
    }

    private void Update()
    {
        var player = Player_Control.Instance;

        // Dizzy UI
        if (player.IsDizzy)
        {
            dizzyPanel.SetActive(true);
            dizzyText.text = player.DizzyAmount.ToString("0");
        }
        else
        {
            dizzyPanel.SetActive(false);
        }

        if (hpFrontImage != null)
        {
            hpFrontImage.fillAmount = player.CurrentHP / player.maxHP;
        }
        // Score UI
        scoreText.text = string.Format("{0:#,0}", GameManager.instance.score);

        staminaImage.fillAmount = player.currentStamina / player.maxStamina;

        if (!gaugeVisible)
        {
            skillPanel.SetActive(false);
            return;
        }

        // Lerp(current, target, t)  → t = 0~1 사이 값. Time.deltaTime * speed 로 부드럽게 이동
        skillTimeImage.fillAmount = Mathf.Lerp(
            skillTimeImage.fillAmount,
            targetFill,
            Time.deltaTime * lerpSpeed
        );

        // 게이지가 다 떨어지면 패널 숨김
        if (targetFill <= 0.001f && skillTimeImage.fillAmount <= 0.01f)
        {
            gaugeVisible = false;
            skillPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 외부(스킬 실행 로직)에서 interval마다 호출.  
    /// currentTime ≤ maxTime 이어야 함.
    /// </summary>
    public void UpdateSkillTime(float currentTime, float maxTime)
    {
        // 새 목표값 계산
        targetFill = Mathf.Clamp01(currentTime / maxTime);

        // 게이지가 처음 켜질 때
        if (!gaugeVisible && targetFill > 0f)
        {
            gaugeVisible = true;
            skillPanel.SetActive(true);
            skillTimeImage.fillAmount = targetFill;   // 첫 프레임엔 바로 맞춰 주기
        }
    }

    public void EndSkillTime()
    {
        targetFill = 0f;
        gaugeVisible = false;
    }

    public void PlayGame()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        optionPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void FinishGame()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Time.timeScale = 0f;
        finishPanel.SetActive(true);
        finishScoreText.text = scoreText.text;
    }

    public void GoHome()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Time.timeScale = 1f;
        LoadScene_Control.LoadScene("StartScene");
    }

    public void RestartGame()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        Time.timeScale = 1f;
        LoadScene_Control.LoadScene("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ClickOption()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);
        optionPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PopUpLevel()
    {
        gameLevelText.transform.parent.GetComponent<Animator>().SetTrigger("doSlide");
        gameLevelText.text = GameManager.instance.gameLevel.ToString();
    }
}
