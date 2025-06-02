// ---------------------------------------------------
// Item.cs
// ---------------------------------------------------
using System;
using UnityEngine;

// �������� int type�� Destroy�� ���������,
// Ǯ���� ���� OnDespawn �ݹ�� Despawn() �޼��带 �߰��߽��ϴ�.
public class Item : MonoBehaviour
{
    public ItemType type;

    // Ǯ�� ������ �� ȣ��Ǵ� �ݹ�
    public Action OnDespawn;

    private void OnTriggerEnter(Collider other)
    {
        // DeadZoneETC�� ������ Ǯ�� ����
        if (other.CompareTag(ConstData.DeadZoneETCTag))
        {
            Despawn();
        }
    }

    /// <summary>
    /// OnDespawn �ݹ��� ȣ���Ͽ� Ǯ�� ���ͽ�ŵ�ϴ�.
    /// </summary>
    public void Despawn()
    {
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
