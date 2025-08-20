// Item.cs
using System;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType type;

    // ▶ 프리팹에서 드래그해 넣는 '아이템 효과(SO)'
    [SerializeField] public ItemEffectSO effect;

    // 풀 복귀 콜백
    public Action OnDespawn;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(ConstData.DeadZoneETCTag))
            Despawn();
    }

    /// <summary>풀로 복귀</summary>
    public void Despawn()
    {
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
