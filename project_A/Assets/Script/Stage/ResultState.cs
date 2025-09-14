// ResultState.cs (full)
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultState : IGameState
{
    private readonly GameStateMachine machine;
    private readonly bool isClear;

    private GameObject resultUI;
    private TextMeshProUGUI resultText;
    private Button nextButton;
    private Button retryButton;
    private Button homeButton;

    public ResultState(GameStateMachine machine, bool isClear)
    {
        this.machine = machine;
        this.isClear = isClear;

        // Hook UI (replace with serialized refs if you prefer)
        resultUI = UI_Control.instance.resultUiPanel;
        resultText = UI_Control.instance.resultText;
        nextButton = UI_Control.instance.nextButton;
        retryButton = UI_Control.instance.retryButton;
        homeButton = UI_Control.instance.homeButton;
    }

    public void Enter()
    {
        if (resultUI != null) { resultUI.SetActive(true); }
        if (resultText != null)
        {
            resultText.text = isClear ? "Stage Clear!" : "Stage Failed";
        }

        if (nextButton != null)
        {
            // Next is only meaningful on clear
            nextButton.gameObject.SetActive(isClear);
            nextButton.onClick.AddListener(OnNextClicked);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeClicked);
        }
    }

    public void Update()
    {
    }

    public void Exit()
    {
        if (resultUI != null) { resultUI.SetActive(false); }
        if (nextButton != null) { nextButton.onClick.RemoveListener(OnNextClicked); }
        if (retryButton != null) { retryButton.onClick.RemoveListener(OnRetryClicked); }
        if (homeButton != null) { homeButton.onClick.RemoveListener(OnHomeClicked); }
    }

    private void OnNextClicked()
    {
        // Proceed to next stage (clear only)
        machine.NextStageOrHome();
    }

    private void OnRetryClicked()
    {
        // Legacy behavior: immediate stage restart in place
        machine.StartStage(machine.currentStageIndex);
    }

    private void OnHomeClicked()
    {
        // Always go Home
        machine.GoHome();
    }
}
