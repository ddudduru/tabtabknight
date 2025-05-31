// ---------------------------------------------------
// ObstacleMapEditorWindow.cs (Editor)
// ---------------------------------------------------
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public class ObstacleMapEditorWindow : EditorWindow
{
    private ObstacleMapData mapData;
    private Vector2 scrollPos;

    [MenuItem("Tools/Obstacle Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<ObstacleMapEditorWindow>("Obstacle Map Editor");
    }

    private void OnEnable()
    {
        mapData = new ObstacleMapData();
        mapData.width = 10;
        mapData.height = 5;
        mapData.cells = Enumerable.Repeat(ObstacleType.Empty, mapData.width * mapData.height).ToList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Grid Size", EditorStyles.boldLabel);
        mapData.width = EditorGUILayout.IntField("Width", mapData.width);
        mapData.height = EditorGUILayout.IntField("Height", mapData.height);

        if (GUILayout.Button("Reset Map"))
        {
            mapData.cells = Enumerable.Repeat(ObstacleType.Empty, mapData.width * mapData.height).ToList();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Load Map JSON", Application.dataPath, "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = File.ReadAllText(path);
                mapData = ObstacleMapData.FromJson(json);
            }
        }
        if (GUILayout.Button("Save JSON"))
        {
            string path = EditorUtility.SaveFilePanel("Save Map JSON", Application.dataPath, "ObstacleMap", "json");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, mapData.ToJson());
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        for (int y = 0; y < mapData.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < mapData.width; x++)
            {
                ObstacleType type = mapData.GetCell(x, y);

                Color prevColor = GUI.backgroundColor;
                GUI.backgroundColor = GetColorForType(type);

                if (GUILayout.Button(type.ToString(), GUILayout.Width(60), GUILayout.Height(20)))
                {
                    int next = ((int)type + 1) % System.Enum.GetValues(typeof(ObstacleType)).Length;
                    mapData.SetCell(x, y, (ObstacleType)next);
                }

                GUI.backgroundColor = prevColor;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private Color GetColorForType(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Tree: return Color.green;
            case ObstacleType.Log: return new Color(0.7f, 0.4f, 0.1f); // brownish
            case ObstacleType.Rock: return Color.gray;
            case ObstacleType.Random: return Color.yellow;
            default: return Color.white;
        }
    }
}
#endif