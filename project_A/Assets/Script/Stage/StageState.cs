// StageState.cs
using UnityEngine;

public class StageState : IGameState
{
    private GameStateMachine machine;
    private int chapterIndex0;
    private int stageIndex0;

    public StageState(GameStateMachine machine, int chapterIndex0, int stageIndex0)
    {
        this.machine = machine;
        this.chapterIndex0 = chapterIndex0;
        this.stageIndex0 = stageIndex0;
    }

    public void Enter()
    {
        Debug.Log($"Enter Stage c{chapterIndex0} s{stageIndex0}");
var data = machine.GetStageData0(stageIndex0);

    // 1) 맵 패턴 세트 교체
    var map = MapController.Instance;
    if (map != null && data != null)
    {
        map.ApplyStageMap(data, resetPositions: data.resetMapPositionsOnEnter);
    }

    // 2) 진행/속도/골 연출 리셋
    var lpm = LevelProgressManager3D.Instance;
    if (lpm != null)
    {
        lpm.ResetForStage(data);
    }

    // 3) 스크롤 재개 (MapController.ApplyStageMap 내부에서 이미 Resume 했다면 생략)
    if (map != null)
    {
        map.PauseScroll(false);
    }
        // TODO: spawn/enable player & gameplay hooks
        Player_Control.Instance.InitPlayerSetting();
        MapController.SetWorldSpeed(2f);
        UIManager.Instance.Show<StageInUI>();
    }

    public void Update()
    {
    }

    public void Exit()
    {
        // optional cleanup
        UIManager.Instance.Hide<StageInUI>();
    }
}
