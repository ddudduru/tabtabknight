using UnityEngine;

public interface IEnemyBrain
{
    void Setup(Enemy owner, Transform player);
    // 기본 이동 델타(baseDelta)를 받아서, 상황에 따라 멈추거나/가속해 반환
    Vector3 ModifyMove(Vector3 baseDelta, float dt);
    void OnHit();        // 피격 시 후처리(선택)
    void OnDespawn();    // 풀 복귀 시 정리

    void OnTriggerEnter(Collider other);
}
