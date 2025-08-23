using UnityEngine;

/// <summary>
/// �÷��̾ Forward/��� ������ ���� +Z�� ������ �߰� �Ÿ��� �ڵ� �����Ͽ�
/// RaceProgress�� �����Ѵ�. ���� Player_Control ���� ���� ����.
/// </summary>
[DisallowMultipleComponent]
public class PlayerProgressReporter : MonoBehaviour
{
    [Tooltip("���� ��")]
    public Vector3 worldForward = new Vector3(0, 0, -1);

    private Vector3 _lastPos;

    private void OnEnable()
    {
        _lastPos = transform.position;
    }

    private void LateUpdate()
    {
        if (RaceProgress.Instance == null) return;

        Vector3 p = transform.position;
        float dz = Vector3.Dot(p - _lastPos, worldForward); // ���� ������ ��� +Z ����
        if (dz > 0f) RaceProgress.Instance.AccumulatePlayer(dz);
        _lastPos = p;
    }
}
