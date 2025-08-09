using UnityEngine;

public enum EffectPoolKind
{
    DashVFX,
    ExplosionVFX,
    AreaAttackVFX,
    // �ʿ� �� �� �߰�
}

/// <summary>
/// �� EffectPoolKind�� �����Ǵ� ��ƼŬ/����Ʈ �����հ�
/// �ʱ� Ǯ ũ�⸦ �ν����Ϳ��� �����ϱ� ���� ������ ��ü�Դϴ�.
/// </summary>
[System.Serializable]
public class EffectPoolConfig
{
    public EffectPoolKind kind;      // Ű ��
    public GameObject prefab;        // Ǯ���� ��ƼŬ������Ʈ ������
    public int initialPoolSize = 5;  // �ʱ� Ǯ ũ��
}