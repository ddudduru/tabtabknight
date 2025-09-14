// HomeState.cs
using UnityEngine;
using UnityEngine.UI;

public class HomeState : IGameState
{
    private GameStateMachine machine;
    // 예시로 메인 메뉴 Canvas나 Panel, 버튼에 대한 참조를 가정
    private GameObject homeUI;
    private Button startButton;
    // (필요 시) 챕터 선택 UI 요소들도 있을 수 있음
    private bool isReturn = false;

    public HomeState(GameStateMachine machine,bool isReturn=false)
    {
        this.machine = machine;
        // 메인 메뉴 UI 객체들을 찾아서 참조 (싱글톤 UI 매니저 사용 가능)
        homeUI = UI_Control.instance.homeUiPanel;
        startButton = UI_Control.instance.startButton;
        this.isReturn = isReturn;
    }

    public void Enter()
    {
        // 메인 메뉴 UI 활성화
        if (homeUI != null) homeUI.SetActive(true);

        // 시작 버튼 이벤트 등록: 버튼 클릭 시 GameStateMachine.StartGame 호출
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
        }
        // 필요하면 현재 챕터 정보 초기화나 배경음 재생 등을 수행
        if(isReturn==false)StartSequenceController.Instance.Sit();
        else StartSequenceController.Instance.Return();
    }

    private void OnStartButtonPressed()
    {
        // "게임 시작" 버튼 눌렀을 때 호출되는 콜백
        // 현재 챕터의 첫 스테이지를 시작
        machine.StartGame();
    }

    public void Update()
    {
        // 메인 메뉴 상태에서 특별히 지속적으로 처리할 로직이 없으면 비워둘 수 있음
        // (예: 배경 애니메이션 처리나 입력 단축키 감지 등이 필요하면 구현)
    }

    public void Exit()
    {
        // 메인 메뉴 UI 비활성화
        if (homeUI != null) homeUI.SetActive(false);
        // 버튼 리스너 해제
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonPressed);
        }
    }
}
