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

    public ChapterDataSO GetChapterByID(int chapterID)
    {
        if (chapters == null) { return null; }
        if (chapterID == 0){ return null; }

        for(int i=0;i< chapters.Length; i++)
        {
            var chapter = chapters[i];
            if (chapter != null)
            {
                if(chapter.chapterId == chapterID)
                {
                    return chapter;
                }
            }
        }
        return null;
    }
}