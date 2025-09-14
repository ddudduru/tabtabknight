// GameStateMachine.cs
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }

    [Header("Content Catalog (SSOT)")]
    public ChapterSetSO chapterSet;          // assign in Inspector

    [Header("Startup")]
    public bool resumeFromLast = true;       // continue from last saved stage

    [Header("Result Behavior")]
    [Tooltip("If true, pressing 'Retry' on Result screen returns to HomeState instead of immediate stage restart.")]
    public bool resultRetryGoesHome = true;

    // 0-based indices
    public int currentChapterIndex = 0;
    public int currentStageIndex = 0;

    public IGameState _current;
    private PlayerProgress _progress;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Load local progress once at boot
        _progress = LocalProgressStorage.Load(chapterSet);

        if (resumeFromLast)
        {
            currentChapterIndex = _progress.chapterIndex0;
            currentStageIndex = _progress.stageIndex0;
        }
        else
        {
            currentChapterIndex = 0;
            currentStageIndex = 0;
            _progress.chapterIndex0 = 0;
            _progress.stageIndex0 = 0;
            LocalProgressStorage.Save(_progress);
        }

        ChangeState(new HomeState(this));
    }

    public void ChangeState(IGameState next)
    {
        if (next == null)
        {
            return;
        }

        if (_current != null)
        {
            _current.Exit();
        }

        _current = next;
        _current.Enter();
    }

    // ===== SSOT helpers =====
    public int GetChapterCount()
    {
        return (chapterSet != null) ? chapterSet.ChapterCount() : 0;
    }

    public ChapterDataSO CurrentChapter()
    {
        if (chapterSet == null)
        {
            return null;
        }
        return chapterSet.GetChapterByIndex(currentChapterIndex);
    }

    public int GetStageCountInCurrentChapter()
    {
        var ch = CurrentChapter();
        if (ch == null || ch.stages == null)
        {
            return 0;
        }
        return ch.stages.Length;
    }

    public StageMapDataSO GetStageData0(int stageIndex0)
    {
        var ch = CurrentChapter();
        if (ch == null || ch.stages == null)
        {
            return null;
        }
        if (stageIndex0 < 0 || stageIndex0 >= ch.stages.Length)
        {
            return null;
        }
        return ch.stages[stageIndex0];
    }

    public bool IsLastStageInChapter()
    {
        int count = GetStageCountInCurrentChapter();
        return currentStageIndex >= (count - 1);
    }

    // ===== Flow API =====
    public void GoHome()
    {
        WorldCleanup.CleanupForHome();
        ChangeState(new HomeState(this,true));
    }

    public void StartGame()
    {
        // Start from saved position (or from 0 if resumeFromLast=false)
        StartStage(currentStageIndex);
    }

    public void StartStage(int stageIndex0)
    {
        WorldCleanup.CleanupForRestart();

        // Clamp to data
        int maxIdx = Mathf.Max(0, GetStageCountInCurrentChapter() - 1);
        currentStageIndex = Mathf.Clamp(stageIndex0, 0, maxIdx);

        // Update progress current cursor and save
        _progress.chapterIndex0 = currentChapterIndex;
        _progress.stageIndex0 = currentStageIndex;
        LocalProgressStorage.Save(_progress);

        ChangeState(new StageState(this, currentChapterIndex, currentStageIndex));
    }

    public void OnStageResult(bool isClear)
    {
        if (MapController.Instance != null)
        {
            MapController.Instance.PauseScroll(true);
        }

        // Update progress only on clear
        if (isClear)
        {
            // Ensure array sizes and clamps
            LocalProgressStorage.ClampAndFix(chapterSet, _progress);

            int stageCount = GetStageCountInCurrentChapter();
            int curCleared = _progress.maxStageCleared0PerChapter[currentChapterIndex];
            if (currentStageIndex > curCleared)
            {
                _progress.maxStageCleared0PerChapter[currentChapterIndex] = currentStageIndex;
            }

            // If last stage cleared, mark chapter cleared
            if (currentStageIndex >= (stageCount - 1))
            {
                if (currentChapterIndex > _progress.maxChapterCleared0)
                {
                    _progress.maxChapterCleared0 = currentChapterIndex;
                }
            }

            LocalProgressStorage.Save(_progress);
        }

        ChangeState(new ResultState(this, isClear));
    }

    public void NextStageOrHome()
    {
        int stageCount = GetStageCountInCurrentChapter();

        if (currentStageIndex < stageCount - 1)
        {
            currentStageIndex += 1;
            _progress.stageIndex0 = currentStageIndex;
            _progress.chapterIndex0 = currentChapterIndex;
            LocalProgressStorage.Save(_progress);

            WorldCleanup.CleanupForRestart();
            StartStage(currentStageIndex);
            return;
        }

        // Chapter complete -> Home
        ChangeState(new HomeState(this,true));
    }

    // Optional utility for QA
    public void ResetAllProgressForDebug()
    {
        _progress = LocalProgressStorage.Reset(chapterSet);
        currentChapterIndex = 0;
        currentStageIndex = 0;
    }
}
