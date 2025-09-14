using UnityEngine;

public interface IGameState
{
    void Enter();   // 상태 진입 시 초기화 작업
    void Update();  // (필요시) 매 프레임 상태별 업데이트 처리
    void Exit();    // 상태 종료 시 정리 작업
}