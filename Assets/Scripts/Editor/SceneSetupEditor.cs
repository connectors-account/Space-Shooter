#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically create the required scenes with the bootstrap objects.
/// Use: Menu > Space Shooter > Setup Scenes
/// </summary>
public class SceneSetupEditor
{
    [MenuItem("Space Shooter/Setup All Scenes")]
    public static void SetupAllScenes()
    {
        CreateMainMenuScene();
        CreateGameScene();
        SetupBuildSettings();
        Debug.Log("✅ All scenes created and build settings configured!");
    }

    [MenuItem("Space Shooter/Create Main Menu Scene")]
    public static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create bootstrap object
        GameObject setup = new GameObject("MainMenuSetup");
        setup.AddComponent<MainMenuSceneSetup>();

        // Save scene
        string path = "Assets/Scenes/MainMenuScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Created: {path}");
    }

    [MenuItem("Space Shooter/Create Game Scene")]
    public static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create bootstrap object
        GameObject setup = new GameObject("GameSetup");
        setup.AddComponent<GameSceneSetup>();

        // Save scene
        string path = "Assets/Scenes/GameScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"Created: {path}");
    }

    [MenuItem("Space Shooter/Setup Build Settings")]
    public static void SetupBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenuScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings updated with MainMenuScene and GameScene.");
    }

    [MenuItem("Space Shooter/Setup Tags and Layers")]
    public static void SetupTagsAndLayers()
    {
        // Add required tags
        AddTag("Player");
        AddTag("Enemy");
        AddTag("Bullet");
        Debug.Log("Tags configured.");
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                return;
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
#endif
