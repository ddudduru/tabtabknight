// ChapterDataSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Stage/Chapter")]
public class ChapterDataSO : ScriptableObject
{
    public int chapterId;
    public string displayName;
    public string description;
    public AudioClip bgm;            // optional
    public Sprite thumbnail;         // optional

    public StageMapDataSO[] stages;  // SSOT for stage count
}
