
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Newtonsoft.Json;

[InitializeOnLoad]
public class LevelEditor : Editor
{
    #region Variables
    private const int REMOVE_TOOL_ID = 0;
    private const int PAINT_TOOL_ID = 1;
    private const string LEVELS_PATH = "Assets/Resources/Levels/";
    private static string levelName = "";

    private static LevelGenerator generator;
    private static Level level;
    #endregion

    #region Editor Prefs
    public static string SelectedObjectId
    {
        get
        {
            return EditorPrefs.GetString("SelectedObjectId", "delault");
        }
        set
        {
            EditorPrefs.SetString("SelectedObjectId", value);
        }
    }


    public static int SelectedTool
    {
        get
        {
            return EditorPrefs.GetInt("SelectedEditorTool", 0);
        }
        set
        {
            EditorPrefs.SetInt("SelectedEditorTool", value);            
        }
    }

    #endregion

    #region Init/Unity Callbacks

    [MenuItem("Editor/Init")]
    public static void InitEditor()
    {       
        generator = FindObjectOfType<LevelGenerator>();      
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        GUI.color = Color.white;
        DrawRightPanel(sceneView);
        if (level == null)
            return;
        DrawLeftPanel(sceneView);
        DrawBottomMenu(sceneView);
        HandleMouseEvents();
    }

    #endregion

    #region Draw Methods

    static void DrawRightPanel(SceneView sceneView)
    {
        Handles.BeginGUI();
        GUI.Box(new Rect(sceneView.position.width - 180, 0, 180, sceneView.position.height), GUIContent.none, EditorStyles.textArea);
        DrawSaveLoadButtons(sceneView);
        Handles.EndGUI();
    }

    private static void DrawSaveLoadButtons(SceneView sceneView)
    {
        GUI.Label(new Rect(sceneView.position.width - 170, 100, 100, 20), "SAVE/LOAD");
        GUI.Label(new Rect(sceneView.position.width - 170, 120, 50, 20), "NAME");
        GUI.contentColor = Color.white;
        levelName = EditorGUI.TextField(new Rect(sceneView.position.width - 130, 120, 100, 20), levelName);
        if (GUI.Button(new Rect(sceneView.position.width - 170, 150, 50, 20), "SAVE"))
        {
            SaveLevel();
        }

        if (GUI.Button(new Rect(sceneView.position.width - 115, 150, 50, 20), "LOAD"))
        {
            LoadLevel();
        }

        if (GUI.Button(new Rect(sceneView.position.width - 60, 150, 50, 20), "NEW"))
        {
            NewLevel();
        }
    }

    static void DrawLeftPanel(SceneView sceneView)
    {
        Handles.BeginGUI();
        GUI.Box(new Rect(sceneView.position.width - 320, 0, 140, sceneView.position.height - 35), GUIContent.none, EditorStyles.textArea);
        DrawObjectButtons(sceneView);
        Handles.EndGUI();
    }

    private static void DrawObjectButtons(SceneView sceneView)
    {
        GUI.Label(new Rect(sceneView.position.width - 280, 20, 100, 20), "TILES");
        for (int i = 0; i < generator.AvaliableObjects.Length; i++)
        {
            DrawObjectButton(i, sceneView.position);
        }
    }

    private static void DrawObjectButton(int index, Rect sceneView)
    {
        string objectName = generator.AvaliableObjects[index].name;
        bool isActive = false;
        if (objectName == SelectedObjectId)
        {
            isActive = true;
        }

        var assetTexture = AssetPreview.GetAssetPreview(generator.AvaliableObjects[index].gameObject);
        GUIContent buttonContent = new GUIContent(assetTexture, objectName);

        Color defaultColor = GUI.color;
        float xOffset = 0;
        int tempIndex = index;

        if (index >= generator.AvaliableObjects.Length / 2)
        {
            xOffset = 60;
            tempIndex = index - Mathf.FloorToInt(generator.AvaliableObjects.Length / 2);
        }

        bool isToggleDown = GUI.Toggle(new Rect(sceneView.width - 310 + xOffset + 5, 50 + tempIndex * 72 + 5, 50, 50), isActive, buttonContent, GUI.skin.button);
        GUI.color = Color.white;
        GUI.Label(new Rect(sceneView.width - 310 + xOffset + 5, 50 + tempIndex * 72 + 55, 50, 20), objectName);
        GUI.color = defaultColor;

        if (isToggleDown == true && isActive == false)
        {
            SelectedObjectId = objectName;
        }
    }


    static void DrawBottomMenu(SceneView sceneView)
    {
        Handles.BeginGUI();
        DrawToolsPanel(sceneView);
        Handles.EndGUI();
    }

    private static void DrawToolsPanel(SceneView sceneView)
    {
        GUILayout.BeginArea(new Rect(0, sceneView.position.height - 35, sceneView.position.width, 20), EditorStyles.toolbar);
        {
            string[] buttonLabels = new string[] { "Remove", "Paint"};

            SelectedTool = GUILayout.SelectionGrid(
                SelectedTool,
                buttonLabels,
                2,
                EditorStyles.toolbarButton,
                GUILayout.Width(600));
        }
        GUILayout.EndArea();
    }

    #endregion

    #region Tools

    static void HandleMouseEvents()
    {    
        if (IsSceneClicked())
        {
            if (SelectedTool == PAINT_TOOL_ID)
                AddObject();
            else if (SelectedTool == REMOVE_TOOL_ID)
                RemoveObject();
        }       
    }

    private static bool IsSceneClicked()
    {
        return Event.current.type == EventType.MouseDown &&
                    Event.current.button == 0 &&
                    Event.current.alt == false &&
                    Event.current.shift == false &&
                    Event.current.control == false;
    }

    private static void RemoveObject()
    {
        Ray ray = Camera.current.ScreenPointToRay(
                      new Vector3(Event.current.mousePosition.x,
                      -Event.current.mousePosition.y + Camera.current.pixelHeight));
        if (Physics.Raycast(ray, out RaycastHit hit, 1000))
        {
            if (hit.collider.gameObject)
            {
                level.TryToRemoveObject(hit.collider.gameObject);
            }
        }
    }

    private static void AddObject()
    {
        Ray ray = Camera.current.ScreenPointToRay(
                        new Vector3(Event.current.mousePosition.x,
                        -Event.current.mousePosition.y + Camera.current.pixelHeight));

        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 30);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000, generator.GroundLayer))
        {
            if (hit.collider.gameObject)
            {
                Level.LevelObjectData data = new Level.LevelObjectData(hit.point, Quaternion.identity, Vector3.one, SelectedObjectId);
                generator.CreateNewObject(data);

            }
        }

    }
  

    #endregion

    #region Level Creation

    private static void NewLevel()
    {
        level = CreateInstance<Level>();

        level.levelObjects = new List<Level.LevelObjectData>();
        AssetDatabase.CreateAsset(level, LEVELS_PATH + levelName + ".asset");
        generator.GenerateLevel(level);
    }

    private static void LoadLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Level name empty", "Set level name before loading", "YEP", "YEP");
            return;
        }

        level = null;
        TextAsset textAssetMap = Resources.Load<TextAsset>("Levels/" + levelName);
        Level loadedLevel = Resources.Load<Level>("Levels/" + levelName);
        if (textAssetMap != null)
        {
            CreateAssetIfNotExisting();

        }
        else if (loadedLevel != null)
        {
            level = loadedLevel;
            CreateTextAsset();
        }
        generator.GenerateLevel(level);
    }

    private static void CreateTextAsset()
    {
        string data = level.ToJson();
        string mydocpath = Application.dataPath + "/Resources/Levels/";
        using (StreamWriter writer = new StreamWriter(Path.Combine(mydocpath, levelName + ".txt")))
        {
            writer.Write(data);
        }
        AssetDatabase.Refresh();
        EditorUtility.SetDirty(level);
    }

    private static void CreateAssetIfNotExisting()
    {
        string data = Resources.Load<TextAsset>("Levels/" + levelName).text;
        if (level == null)
        {
            level = CreateInstance<Level>();
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            level = JsonConvert.DeserializeObject<Level>(data, settings);
            if (!string.IsNullOrEmpty(data))
                AssetDatabase.CreateAsset(level, "Assets/Resources/Levels/" + levelName + ".asset");
        }
    }

    [MenuItem("Editor/Save Level &s")]
    private static void SaveLevel()
    {
        if (string.IsNullOrEmpty(levelName))
        {
            EditorUtility.DisplayDialog("Level name empty", "Set level name before saving", "YEP", "YEP");
            return;
        }
        UpdateLevel();
        CreateTextAsset();
    }

    private static void UpdateLevel()
    {
        foreach (var obj in level.levelObjects)
        {
            obj.position = obj.instantiatedObject.transform.position;
            obj.rotation = obj.instantiatedObject.transform.rotation;
            obj.scale = obj.instantiatedObject.transform.localScale;
        }
    }

   
    #endregion
}