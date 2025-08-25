// ---------------------------------------------------
// WormProjectile.cs
// ---------------------------------------------------
using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class WormProjectile : MonoBehaviour
{
    private Vector3 dir;
    private float speed;
    private float life;
    private float dizzyOnHit;

    private Transform parentForScroll; // �� ��Ʈ(��ũ�� ���)
    private Action<WormProjectile> onDespawn;

    [SerializeField] private bool freezeWithWorld = false; // ���� �� ������

    public void Init(Action<WormProjectile> onDespawn)
    {
        this.onDespawn = onDespawn;
    }

    public void Launch(Vector3 pos, Vector3 dir, float speed, float life, float dizzyOnHit, Transform parentForScroll)
    {
        this.gameObject.SetActive(true);
        this.dir = new Vector3(dir.x, 0f, dir.z).normalized; // �� Y ����(���� ����)
        this.speed = speed;
        this.life = life;
        this.dizzyOnHit = dizzyOnHit;

        transform.SetParent(parentForScroll, true);
        transform.position = pos;

        // ó������ ���� ������ �ٶ󺸰� (yaw��)
        transform.rotation = Quaternion.LookRotation(this.dir, Vector3.up);

        gameObject.SetActive(true);
    }
    void Update()
    {
        if (life <= 0f) { Despawn(); return; }
        life -= Time.deltaTime;

        transform.position += ((dir * speed * Time.deltaTime));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ConstData.PlayerTag))
        {
            Player_Control.Instance?.HitPlayer(dizzyOnHit);
            Despawn();
        }
        else if (other.CompareTag(ConstData.ObstacleTag) || other.CompareTag(ConstData.DeadZoneETCTag))
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        onDespawn?.Invoke(this);
    }
}
