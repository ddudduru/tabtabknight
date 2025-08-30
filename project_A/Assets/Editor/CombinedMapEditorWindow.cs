// ---------------------------------------------------
// CombinedMapEditorWindow.cs (Editor)
// ---------------------------------------------------
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public class CombinedMapEditorWindow : EditorWindow
{
    private CombinedMapData mapData;
    private Vector2 scrollPos;

    private enum EditLayer { Obstacle, Item, Monster }
    private EditLayer currentLayer = EditLayer.Obstacle;

    private ObstacleType bevelObstacle = ObstacleType.Tree;
    private ItemType bevelItem = ItemType.Skill;
    private MonsterType bevelMonster = MonsterType.Ghost;

    private bool isErasing = false;
    private bool isPainting = false;

    [MenuItem("Tools/Combined Map Editor")]
    public static void ShowWindow()
    {
        GetWindow<CombinedMapEditorWindow>("Combined Map Editor");
    }

    private void OnEnable()
    {
        mapData = new CombinedMapData(10, 5);
    }

    private void OnGUI()
    {
        // 1) 레이어 선택
        EditorGUILayout.LabelField("Edit Layer", EditorStyles.boldLabel);
        currentLayer = (EditLayer)GUILayout.Toolbar(
            (int)currentLayer,
            new string[] { "Obstacle", "Item", "Monster" }
        );

        EditorGUILayout.Space();

        // 2) 그리드 크기 입력
        int newWidth = EditorGUILayout.IntField("Width", mapData.width);
        int newHeight = EditorGUILayout.IntField("Height", mapData.height);
        if (newWidth != mapData.width || newHeight != mapData.height)
        {
            newWidth = Mathf.Max(1, newWidth);
            newHeight = Mathf.Max(1, newHeight);
            mapData = new CombinedMapData(newWidth, newHeight);
        }
        if (GUILayout.Button("Reset Map"))
        {
            mapData = new CombinedMapData(mapData.width, mapData.height);
        }

        EditorGUILayout.Space();

        // 3) Load / Save 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load JSON"))
        {
            string path = EditorUtility.OpenFilePanel("Load Combined JSON", Application.dataPath+ "Data/MapData", "json");
            if (!string.IsNullOrEmpty(path))
            {
                string json = File.ReadAllText(path);
                mapData = CombinedMapData.FromJson(json);
            }
        }
        if (GUILayout.Button("Save JSON"))
        {
            string path = EditorUtility.SaveFilePanel("Save Combined JSON", Application.dataPath + "Data/MapData", "CombinedMapData", "json");
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, mapData.ToJson());
                AssetDatabase.Refresh();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 4) 브러시 설정
        EditorGUILayout.LabelField("Brush Settings", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            switch (currentLayer)
            {
                case EditLayer.Obstacle:
                    bevelObstacle = (ObstacleType)EditorGUILayout.EnumPopup(
                        new GUIContent("Obstacle Brush", "클릭/드래그 시 채울 장애물 타입"),
                        bevelObstacle
                    );
                    break;

                case EditLayer.Item:
                    bevelItem = (ItemType)EditorGUILayout.EnumPopup(
                        new GUIContent("Item Brush", "클릭/드래그 시 채울 아이템 타입"),
                        bevelItem
                    );
                    break;

                case EditLayer.Monster:
                    bevelMonster = (MonsterType)EditorGUILayout.EnumPopup(
                        new GUIContent("Monster Brush", "클릭/드래그 시 채울 몬스터 타입"),
                        bevelMonster
                    );
                    break;
            }

            Color prevColor = GUI.backgroundColor;
            GUI.backgroundColor = isErasing ? Color.red : GUI.backgroundColor;
            if (GUILayout.Button(new GUIContent("Erase Mode", "활성화 시 클릭/드래그로 빈칸으로 설정"), GUILayout.Width(100)))
            {
                isErasing = !isErasing;
            }
            GUI.backgroundColor = prevColor;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 5) 그리드 출력 (스크롤 영역)
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int y = 0; y < mapData.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < mapData.width; x++)
            {
                string label;
                Color bg;

                if (currentLayer == EditLayer.Obstacle)
                {
                    ObstacleType t = mapData.GetObstacle(x, y);
                    label = t.ToString();
                    bg = GetColorForObstacle(t);
                }
                else if (currentLayer == EditLayer.Item)
                {
                    ItemType t = mapData.GetItem(x, y);
                    label = t.ToString();
                    bg = GetColorForItem(t);
                }
                else // Monster 레이어
                {
                    MonsterType t = mapData.GetMonster(x, y);
                    label = t.ToString();
                    bg = GetColorForMonster(t);
                }

                Color prevBG = GUI.backgroundColor;
                GUI.backgroundColor = bg;

                if (GUILayout.Button(label, GUILayout.Width(60), GUILayout.Height(20)))
                {
                    if (Event.current.control)
                    {
                        if (currentLayer == EditLayer.Obstacle)
                        {
                            ObstacleType old = mapData.GetObstacle(x, y);
                            int next = ((int)old + 1) % System.Enum.GetValues(typeof(ObstacleType)).Length;
                            mapData.SetObstacle(x, y, (ObstacleType)next);
                        }
                        else if (currentLayer == EditLayer.Item)
                        {
                            ItemType old = mapData.GetItem(x, y);
                            int next = ((int)old + 1) % System.Enum.GetValues(typeof(ItemType)).Length;
                            mapData.SetItem(x, y, (ItemType)next);
                        }
                        else
                        {
                            MonsterType old = mapData.GetMonster(x, y);
                            int next = ((int)old + 1) % System.Enum.GetValues(typeof(MonsterType)).Length;
                            mapData.SetMonster(x, y, (MonsterType)next);
                        }
                    }
                    else
                    {
                        if (isErasing)
                        {
                            if (currentLayer == EditLayer.Obstacle)
                                mapData.SetObstacle(x, y, ObstacleType.Empty);
                            else if (currentLayer == EditLayer.Item)
                                mapData.SetItem(x, y, ItemType.None);
                            else
                                mapData.SetMonster(x, y, MonsterType.None);
                        }
                        else
                        {
                            if (currentLayer == EditLayer.Obstacle)
                                mapData.SetObstacle(x, y, bevelObstacle);
                            else if (currentLayer == EditLayer.Item)
                                mapData.SetItem(x, y, bevelItem);
                            else
                                mapData.SetMonster(x, y, bevelMonster);
                        }
                    }
                    Repaint();
                }

                Rect btnRect = GUILayoutUtility.GetLastRect();
                HandlePaintingEvent(btnRect, x, y);

                GUI.backgroundColor = prevBG;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private Color GetColorForObstacle(ObstacleType type)
    {
        switch (type)
        {
            case ObstacleType.Tree: return Color.green;
            case ObstacleType.Log: return new Color(0.7f, 0.4f, 0.1f);
            case ObstacleType.Rock: return Color.gray;
            case ObstacleType.FallingRock: return Color.magenta;
            case ObstacleType.Random: return Color.yellow;
            default: return Color.white;
        }
    }

    private Color GetColorForItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.None: return Color.white;
            case ItemType.Skill: return Color.cyan;
            case ItemType.Forward: return Color.yellow;
            case ItemType.Coin: return Color.green;
            case ItemType.Magnet: return Color.blue;
            case ItemType.Random: return new Color(1f, 0.4f, 1f);
            default: return Color.white;
        }
    }

    private Color GetColorForMonster(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.None: return Color.white;
            case MonsterType.Ghost: return new Color(1f, 0.5f, 0f);
            case MonsterType.Skeleton: return Color.gray;
            case MonsterType.Bat: return new Color(0.6f, 0f, 0.6f);
            case MonsterType.Slime: return new Color(0.2f, 1f, 0.2f);
            case MonsterType.Crab: return Color.red;
            case MonsterType.Worm: return Color.coral;
            case MonsterType.Random: return Color.magenta;
            default: return Color.white;
        }
    }

    private void HandlePaintingEvent(Rect btnRect, int x, int y)
    {
        Event e = Event.current;
        Vector2 mousePos = e.mousePosition;

        if (e.type == EventType.MouseDown && e.button == 0 && btnRect.Contains(mousePos))
        {
            isPainting = true;
            ApplyBrush(x, y);
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isPainting)
        {
            if (btnRect.Contains(mousePos))
                ApplyBrush(x, y);
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            isPainting = false;
        }
    }

    private void ApplyBrush(int x, int y)
    {
        if (isErasing)
        {
            if (currentLayer == EditLayer.Obstacle)
                mapData.SetObstacle(x, y, ObstacleType.Empty);
            else if (currentLayer == EditLayer.Item)
                mapData.SetItem(x, y, ItemType.None);
            else
                mapData.SetMonster(x, y, MonsterType.None);
        }
        else
        {
            if (currentLayer == EditLayer.Obstacle)
                mapData.SetObstacle(x, y, bevelObstacle);
            else if (currentLayer == EditLayer.Item)
                mapData.SetItem(x, y, bevelItem);
            else
                mapData.SetMonster(x, y, bevelMonster);
        }
        Repaint();
    }
}
#endif
