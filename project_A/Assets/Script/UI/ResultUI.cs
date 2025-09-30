using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : UIBase
{
    [Header("Refs")]
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button homeButton;

    // cache of the machine passed via OpenParam
    private GameStateMachine machine;
    private bool isClear;

    public struct OpenParam
    {
        public GameStateMachine Machine;   // required: who to call on button actions
        public bool IsClear;               // controls next button visibility
        public string Title;               // optional: "Stage Clear!" or "Stage Failed"
        public string Score;               // optional: formatted score string
        // public int StageIndex;          // optional: if you want to override retry target
    }

    public override void OnPreload()
    {
        // no-op
    }

    protected override void Awake()
    {
        base.Awake();

        // lazy find if not assigned
        if (resultText == null || scoreText == null)
        {
            foreach (var t in GetComponentsInChildren<TMP_Text>(true))
            {
                var n = t.name.ToLower();
                if (n.Contains("result")) resultText = t;
                else if (n.Contains("score")) scoreText = t;
            }
        }
        if (retryButton == null || nextButton == null || homeButton == null)
        {
            foreach (var b in GetComponentsInChildren<Button>(true))
            {
                var n = b.name.ToLower();
                if (n.Contains("retrybtn")) retryButton = b;
                else if (n.Contains("nextbtn")) nextButton = b;
                else if (n.Contains("homebtn")) homeButton = b;
            }
        }

        // static wiring to local handlers (no machine reference here yet)
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(OnClickRetry);
        }
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
        }
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(OnClickHome);
        }
    }

    public override void Show(object param = null)
    {
        base.Show(param);

        // default fallbacks
        machine = null;
        isClear = false;
        string title = "Result";
        string score = string.Format("{0:#,0}", GameManager.instance.score);

        if (param is OpenParam p)
        {
            machine = p.Machine;
            isClear = p.IsClear;
            if (!string.IsNullOrEmpty(p.Title)) title = p.Title;
            if (!string.IsNullOrEmpty(p.Score)) score = p.Score;
        }
        else
        {
            // if caller didn't pass OpenParam, infer title from game context if desired
            // title = GameManager.instance.IsClear ? "Stage Clear!" : "Stage Failed";
        }

        if (resultText != null) resultText.text = title;
        if (scoreText != null) scoreText.text = score;

        // show/hide Next button based on clear
        if (nextButton != null) nextButton.gameObject.SetActive(isClear);
    }

    public override void Hide()
    {
        base.Hide();
        // nothing else to clean; listeners are static, machine reference is thrown away on next Show
    }

    private void OnClickRetry()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);

        if (machine != null)
        {
            // mirror legacy behavior
            machine.StartStage(machine.currentStageIndex);
        }
        else
        {
            // fallback
            LoadScene_Control.LoadScene("GameScene");
        }
    }

    private void OnClickNext()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);

        if (machine != null)
        {
            machine.NextStageOrHome();
        }
        else
        {
            LoadScene_Control.LoadScene("GameScene");
        }
    }

    private void OnClickHome()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);

        if (machine != null)
        {
            machine.GoHome();
        }
        else
        {
            LoadScene_Control.LoadScene("StartScene");
        }
    }
}
