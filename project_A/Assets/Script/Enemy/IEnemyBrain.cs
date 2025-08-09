using UnityEngine;

public interface IEnemyBrain
{
    void Setup(Enemy owner, Transform player);
    // �⺻ �̵� ��Ÿ(baseDelta)�� �޾Ƽ�, ��Ȳ�� ���� ���߰ų�/������ ��ȯ
    Vector3 ModifyMove(Vector3 baseDelta, float dt);
    void OnHit();        // �ǰ� �� ��ó��(����)
    void OnDespawn();    // Ǯ ���� �� ����

    void OnTriggerEnter(Collider other);
}
