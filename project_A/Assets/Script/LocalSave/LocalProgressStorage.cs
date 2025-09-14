// LocalProgressStorage.cs
using System;
using System.IO;
using UnityEngine;

public static class LocalProgressStorage
{
    private const string FileName = "player_progress.json";
    private const string PkChapter = "pp.chapterIndex0";
    private const string PkStage = "pp.stageIndex0";

    private static string FilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }
    }

    public static PlayerProgress Load(ChapterSetSO set)
    {
        int chapterCount = (set != null) ? set.ChapterCount() : 1;
        PlayerProgress p = null;

        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                if (!string.IsNullOrEmpty(json))
                {
                    p = JsonUtility.FromJson<PlayerProgress>(json);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Load progress failed. Fallback to PlayerPrefs. {e.Message}");
        }

        // Fallback: PlayerPrefs minimal
        if (p == null)
        {
            if (PlayerPrefs.HasKey(PkChapter) && PlayerPrefs.HasKey(PkStage))
            {
                p = PlayerProgress.Default(chapterCount);
                p.chapterIndex0 = PlayerPrefs.GetInt(PkChapter, 0);
                p.stageIndex0 = PlayerPrefs.GetInt(PkStage, 0);
            }
        }

        if (p == null)
        {
            p = PlayerProgress.Default(chapterCount);
        }

        ClampAndFix(set, p);
        return p;
    }

    public static void Save(PlayerProgress p)
    {
        if (p == null)
        {
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(p, true);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Save progress failed. Using PlayerPrefs fallback. {e.Message}");
            PlayerPrefs.SetInt(PkChapter, p.chapterIndex0);
            PlayerPrefs.SetInt(PkStage, p.stageIndex0);
            PlayerPrefs.Save();
        }
    }

    public static PlayerProgress Reset(ChapterSetSO set)
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Reset file delete failed: {e.Message}");
        }

        PlayerPrefs.DeleteKey(PkChapter);
        PlayerPrefs.DeleteKey(PkStage);
        PlayerPrefs.Save();

        var p = PlayerProgress.Default((set != null) ? set.ChapterCount() : 1);
        Save(p);
        return p;
    }

    public static void ClampAndFix(ChapterSetSO set, PlayerProgress p)
    {
        int chapterCount = (set != null) ? set.ChapterCount() : 1;

        // Resize per-chapter array
        if (p.maxStageCleared0PerChapter == null || p.maxStageCleared0PerChapter.Length != chapterCount)
        {
            var newArr = new int[chapterCount];
            for (int i = 0; i < chapterCount; i++)
            {
                int old = -1;
                if (p.maxStageCleared0PerChapter != null && i < p.maxStageCleared0PerChapter.Length)
                {
                    old = p.maxStageCleared0PerChapter[i];
                }
                newArr[i] = old;
            }
            p.maxStageCleared0PerChapter = newArr;
        }

        // Clamp current position
        p.chapterIndex0 = Mathf.Clamp(p.chapterIndex0, 0, Mathf.Max(0, chapterCount - 1));

        int stageCountCur = GetStageCountSafe(set, p.chapterIndex0);
        p.stageIndex0 = Mathf.Clamp(p.stageIndex0, 0, Mathf.Max(0, stageCountCur - 1));

        // Clamp cleared markers
        p.maxChapterCleared0 = Mathf.Clamp(p.maxChapterCleared0, -1, Mathf.Max(-1, chapterCount - 1));

        for (int c = 0; c < chapterCount; c++)
        {
            int sc = GetStageCountSafe(set, c);
            p.maxStageCleared0PerChapter[c] = Mathf.Clamp(p.maxStageCleared0PerChapter[c], -1, Mathf.Max(-1, sc - 1));
        }
    }

    private static int GetStageCountSafe(ChapterSetSO set, int chapterIndex0)
    {
        if (set == null)
        {
            return 1;
        }
        var ch = set.GetChapterByIndex(chapterIndex0);
        if (ch == null || ch.stages == null)
        {
            return 1;
        }
        return Mathf.Max(1, ch.stages.Length);
    }
}
