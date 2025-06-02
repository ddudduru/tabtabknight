// ---------------------------------------------------
// Item.cs
// ---------------------------------------------------
using System;
using UnityEngine;

// 기존에는 int type과 Destroy만 사용했으나,
// 풀링을 위해 OnDespawn 콜백과 Despawn() 메서드를 추가했습니다.
public class Item : MonoBehaviour
{
    public ItemType type;

    // 풀로 복귀할 때 호출되는 콜백
    public Action OnDespawn;

    private void OnTriggerEnter(Collider other)
    {
        // DeadZoneETC에 닿으면 풀로 복귀
        if (other.CompareTag(ConstData.DeadZoneETCTag))
        {
            Despawn();
        }
    }

    /// <summary>
    /// OnDespawn 콜백을 호출하여 풀로 복귀시킵니다.
    /// </summary>
    public void Despawn()
    {
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
