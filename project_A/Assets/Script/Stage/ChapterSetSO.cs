using UnityEngine;

[CreateAssetMenu(menuName = "Game/Chapter Set")]
public class ChapterSetSO : ScriptableObject
{
    public ChapterDataSO[] chapters;

    public int ChapterCount()
    {
        return (chapters != null) ? chapters.Length : 0;
    }

    public ChapterDataSO GetChapterByIndex(int index0)
    {
        if (chapters == null) { return null; }
        if (index0 < 0 || index0 >= chapters.Length) { return null; }
        return chapters[index0];
    }
}