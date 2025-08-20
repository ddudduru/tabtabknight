using UnityEngine;

public class DefaultForwardBrain : IEnemyBrain
{
    Enemy owner; Transform player;
    public void Setup(Enemy owner, Transform player)
    {
        this.owner = owner; this.player = player;
        owner.enemySpeed = 1f;
    }
    public Vector3 ModifyMove(Vector3 baseDelta, float dt) => baseDelta;
    public void OnHit() { }
    public void OnDespawn() { }

    public void OnTriggerEnter(Collider other)
    {

    }
}
