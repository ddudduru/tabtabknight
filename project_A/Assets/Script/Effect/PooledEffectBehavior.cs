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
    /// Ǯ���� ���� �� ȣ��˴ϴ�.
    /// </summary>
    /// <param name="kind">����Ʈ ����(Ǯ Ű)</param>
    /// <param name="parent">���� �θ� Transform (null�̸� ���常)</param>
    /// <param name="localPosition">�θ� null�������ǥ, �ƴϸ� ������ǥ</param>
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
