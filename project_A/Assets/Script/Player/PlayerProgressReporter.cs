using UnityEngine;

/// <summary>
/// 플레이어가 Forward/대시 등으로 실제 +Z로 움직인 추가 거리를 자동 계측하여
/// RaceProgress에 누적한다. 기존 Player_Control 수정 없이 동작.
/// </summary>
[DisallowMultipleComponent]
public class PlayerProgressReporter : MonoBehaviour
{
    [Tooltip("전진 축")]
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
        float dz = Vector3.Dot(p - _lastPos, worldForward); // 지난 프레임 대비 +Z 성분
        if (dz > 0f) RaceProgress.Instance.AccumulatePlayer(dz);
        _lastPos = p;
    }
}
