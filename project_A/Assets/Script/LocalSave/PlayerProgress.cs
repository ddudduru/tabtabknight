// PlayerProgress.cs
using System;
using UnityEngine;

[Serializable]
public class PlayerProgress
{
    // Current position (0-based)
    public int chapterIndex0;
    public int stageIndex0;

    // Highest cleared markers
    public int maxChapterCleared0;               // highest chapter index fully cleared
    public int[] maxStageCleared0PerChapter;     // per-chapter highest cleared stage index

    public static PlayerProgress Default(int chapterCount)
    {
        var p = new PlayerProgress();
        p.chapterIndex0 = 0;
        p.stageIndex0 = 0;
        p.maxChapterCleared0 = -1; // -1 means none
        p.maxStageCleared0PerChapter = new int[Mathf.Max(1, chapterCount)];
        for (int i = 0; i < p.maxStageCleared0PerChapter.Length; i++)
        {
            p.maxStageCleared0PerChapter[i] = -1; // none cleared
        }
        return p;
    }
}
