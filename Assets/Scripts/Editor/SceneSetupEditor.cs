#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor script to help set up the game scene quickly.
/// Accessible via Window > Space Shooter > Setup Scene
/// </summary>
public class SceneSetupEditor : EditorWindow
{
    [MenuItem("Window/Space Shooter/Setup Scene")]
    public static void ShowWindow()
    {
        GetWindow<SceneSetupEditor>("Space Shooter Setup");
    }
    
    [MenuItem("Window/Space Shooter/Create New Game Scene")]
    public static void CreateNewScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Set up camera
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        
        // Create GameInitializer
        GameObject initializer = new GameObject("GameInitializer");
        initializer.AddComponent<GameInitializer>();
        
        // Save scene
        string scenePath = "Assets/Scenes/MainScene.unity";
        System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, scenePath);
        
        Debug.Log("New game scene created and saved to: " + scenePath);
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Space Shooter Scene Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Use these tools to quickly set up your game scene.",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Complete Game Scene", GUILayout.Height(40)))
        {
            CreateNewScene();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Individual Setup", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Add GameInitializer to Scene"))
        {
            AddGameInitializer();
        }
        
        if (GUILayout.Button("Configure Camera"))
        {
            ConfigureCamera();
        }
        
        if (GUILayout.Button("Setup Tags and Layers"))
        {
            SetupTagsAndLayers();
        }
        
        if (GUILayout.Button("Configure Physics2D"))
        {
            ConfigurePhysics();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Build", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Open Build Settings"))
        {
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
        
        if (GUILayout.Button("Add Current Scene to Build"))
        {
            AddCurrentSceneToBuild();
        }
    }
    
    private void AddGameInitializer()
    {
        if (FindObjectOfType<GameInitializer>() != null)
        {
            Debug.LogWarning("GameInitializer already exists in the scene.");
            return;
        }
        
        GameObject initializer = new GameObject("GameInitializer");
        initializer.AddComponent<GameInitializer>();
        Selection.activeGameObject = initializer;
        
        Debug.Log("GameInitializer added to scene.");
    }
    
    private void ConfigureCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("No Main Camera found!");
            return;
        }
        
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.transform.position = new Vector3(0, 0, -10);
        
        Debug.Log("Camera configured for 2D space shooter.");
    }
    
    private void SetupTagsAndLayers()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        
        // Add tags
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        string[] requiredTags = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "PowerUp" };
        
        foreach (string tag in requiredTags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            }
        }
        
        // Add layers
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        string[] requiredLayers = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "PowerUp" };
        int[] layerIndices = { 8, 9, 10, 11, 12 };
        
        for (int i = 0; i < requiredLayers.Length; i++)
        {
            SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(layerIndices[i]);
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = requiredLayers[i];
            }
        }
        
        tagManager.ApplyModifiedProperties();
        
        Debug.Log("Tags and Layers configured.");
    }
    
    private void ConfigurePhysics()
    {
        // Set gravity to zero for space
        Physics2D.gravity = Vector2.zero;
        
        // Configure collision matrix
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerBulletLayer = LayerMask.NameToLayer("PlayerBullet");
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        int powerUpLayer = LayerMask.NameToLayer("PowerUp");
        
        if (playerLayer >= 0 && enemyLayer >= 0)
        {
            // Disable all collisions first, then enable specific ones
            for (int i = 8; i <= 12; i++)
            {
                for (int j = 8; j <= 12; j++)
                {
                    Physics2D.IgnoreLayerCollision(i, j, true);
                }
            }
            
            // Enable specific collisions
            if (playerLayer >= 0 && enemyLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, false);
            if (playerLayer >= 0 && enemyBulletLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, enemyBulletLayer, false);
            if (playerLayer >= 0 && powerUpLayer >= 0)
                Physics2D.IgnoreLayerCollision(playerLayer, powerUpLayer, false);
            if (enemyLayer >= 0 && playerBulletLayer >= 0)
                Physics2D.IgnoreLayerCollision(enemyLayer, playerBulletLayer, false);
            
            Debug.Log("Physics2D collision matrix configured.");
        }
        else
        {
            Debug.LogWarning("Layers not found. Please run 'Setup Tags and Layers' first.");
        }
    }
    
    private void AddCurrentSceneToBuild()
    {
        var currentScene = EditorSceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(currentScene.path))
        {
            Debug.LogWarning("Please save the scene first.");
            return;
        }
        
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        
        bool found = false;
        foreach (var scene in scenes)
        {
            if (scene.path == currentScene.path)
            {
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            scenes.Add(new EditorBuildSettingsScene(currentScene.path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Scene added to build settings: " + currentScene.path);
        }
        else
        {
            Debug.Log("Scene already in build settings.");
        }
    }
}
#endif
