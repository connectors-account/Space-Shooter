using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool to quickly create the MainMenu and GamePlay scenes
/// with all necessary GameObjects pre-configured.
/// Access via menu: Tools > Space Shooter > Setup Scenes
/// </summary>
public class SceneSetupEditor : Editor
{
    [MenuItem("Tools/Space Shooter/Setup All Scenes")]
    public static void SetupAllScenes()
    {
        SetupMainMenuScene();
        SetupGamePlayScene();
        SetupBuildSettings();
        Debug.Log("✅ All scenes created and build settings configured!");
        EditorUtility.DisplayDialog("Setup Complete",
            "Both scenes have been created:\n\n" +
            "• MainMenu (Assets/Scenes/MainMenu.unity)\n" +
            "• GamePlay (Assets/Scenes/GamePlay.unity)\n\n" +
            "Build settings have been configured.\n" +
            "Press Play to start the game!", "OK");
    }

    [MenuItem("Tools/Space Shooter/Setup Main Menu Scene")]
    public static void SetupMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create setup object
        GameObject setup = new GameObject("_MainMenuSetup");
        setup.AddComponent<MainMenuSetup>();

        // Create bootstrapper
        GameObject bootstrapper = new GameObject("_Bootstrapper");
        bootstrapper.AddComponent<GameBootstrapper>();

        // Save
        string path = "Assets/Scenes/MainMenu.unity";
        EnsureDirectoryExists(path);
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"✅ MainMenu scene saved to {path}");
    }

    [MenuItem("Tools/Space Shooter/Setup GamePlay Scene")]
    public static void SetupGamePlayScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Create setup object
        GameObject setup = new GameObject("_GamePlaySetup");
        setup.AddComponent<GamePlaySetup>();

        // Save
        string path = "Assets/Scenes/GamePlay.unity";
        EnsureDirectoryExists(path);
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log($"✅ GamePlay scene saved to {path}");
    }

    [MenuItem("Tools/Space Shooter/Configure Build Settings")]
    public static void SetupBuildSettings()
    {
        // Add scenes to build settings
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GamePlay.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("✅ Build settings configured with MainMenu and GamePlay scenes");
    }

    private static void EnsureDirectoryExists(string assetPath)
    {
        string fullPath = System.IO.Path.GetDirectoryName(
            System.IO.Path.Combine(Application.dataPath, "..", assetPath));
        if (!System.IO.Directory.Exists(fullPath))
            System.IO.Directory.CreateDirectory(fullPath);
    }
}
