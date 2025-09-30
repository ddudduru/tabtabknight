using UnityEngine;

public class HomeState : IGameState
{
    private readonly GameStateMachine machine;
    private readonly bool isReturn;

    public HomeState(GameStateMachine machine, bool isReturn = false)
    {
        this.machine = machine;
        this.isReturn = isReturn;
    }

    public void Enter()
    {
        var p = new HomeUI.OpenParam
        {
            Machine = machine,
            IsReturn = isReturn
        };

        UIManager.Instance.Show<HomeUI>(p);
    }

    public void Update()
    {
        // usually no-op
    }

    public void Exit()
    {
        UIManager.Instance.Hide<HomeUI>();
    }
}
