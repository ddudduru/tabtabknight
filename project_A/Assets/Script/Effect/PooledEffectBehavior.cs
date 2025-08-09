// PooledEffectBehavior.cs
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PooledEffectBehavior : MonoBehaviour
{
    private ParticleSystem ps;
    private EffectPoolKind myKind;
    private Transform parentTransform;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// 풀에서 꺼낼 때 호출됩니다.
    /// </summary>
    /// <param name="kind">이펙트 종류(풀 키)</param>
    /// <param name="parent">붙일 부모 Transform (null이면 월드만)</param>
    /// <param name="localPosition">부모가 null→월드좌표, 아니면 로컬좌표</param>
    public void Initialize(EffectPoolKind kind, Transform parent, Vector3 localPosition)
    {
        myKind = kind;
        parentTransform = parent;

        if (parentTransform != null)
        {
            transform.SetParent(parentTransform, worldPositionStays: false);
            transform.localPosition = localPosition;
        }
        else
        {
            transform.SetParent(null);
            transform.position = localPosition;
        }
        transform.localScale = Vector3.one;

        gameObject.SetActive(true);
        ps.Play();
    }

    public void ReturnToPool()
    {
        CancelInvoke();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        EffectPoolManager.Instance.ReleaseEffect(myKind, gameObject);
    }
}
