// Item.cs
using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType type;

    // �� �����տ��� �巡���� �ִ� '������ ȿ��(SO)'
    [SerializeField] public ItemEffectSO effect;

    // Ǯ ���� �ݹ�
    public Action OnDespawn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ConstData.DeadZoneETCTag))
            Despawn();
    }

    /// <summary>Ǯ�� ����</summary>
    public void Despawn()
    {
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
