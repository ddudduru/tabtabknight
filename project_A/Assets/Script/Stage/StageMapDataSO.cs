// StageMapDataSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Stage/Stage Map Data")]
public class StageMapDataSO : ScriptableObject
{
    public TextAsset[] combinedPatternJsons;
    public TextAsset initCombinedPatternJson;

    [Header("Map Apply Option")]
    public bool resetMapPositionsOnEnter = true;   // 스테이지 진입 시 맵 파트 위치를 리셋할지

    [Header("Gameplay: Progress & Speeds")]
    [Min(0f)] public float targetDistance = 300f;
    [Min(0f)] public float baseMoveSpeed = 5f;
    [Min(0f)] public float deadZoneSpeed = 4f;
    [Range(0f, 1f)] public float goalAppearAt = 0.80f;
    [Min(0f)] public float goalVisibleAhead = 6f;
    [Min(0f)] public float initialDeadGap = 50f;
    public bool hideGoalBeforeAppear = true;

    [Header("Optional: Dizzy Handling")]
    public bool freezeWorldOnDizzy = true;
}
