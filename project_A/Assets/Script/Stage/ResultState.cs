using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultState : IGameState
{
    private readonly GameStateMachine machine;
    private readonly bool isClear;

    public ResultState(GameStateMachine machine, bool isClear)
    {
        this.machine = machine;
        this.isClear = isClear;
    }

    public void Enter()
    {
        // Build params for ResultUI and show it via UIManager
        var p = new ResultUI.OpenParam
        {
            Machine = machine,
            IsClear = isClear,
            Title = isClear ? "Stage Clear!" : "Stage Failed",
            Score = string.Format("{0:#,0}", GameManager.instance.score)
        };

        UIManager.Instance.Show<ResultUI>(p);
    }

    public void Update()
    {
        // usually no-op
    }

    public void Exit()
    {
        UIManager.Instance.Hide<ResultUI>();
    }
}
