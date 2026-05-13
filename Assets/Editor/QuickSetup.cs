// ============================================================================
// QuickSetup.cs - Editor utility to automatically create all three scenes
// Access via Unity menu: Tools > Space Shooter > Quick Setup
// ============================================================================
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;

/// <summary>
/// Editor utility that creates all three game scenes with the correct bootstrap
/// objects and configures Build Settings — one-click project setup.
/// </summary>
public class QuickSetup : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Quick Setup - Create All Scenes")]
    public static void RunQuickSetup()
    {
        if (!EditorUtility.DisplayDialog("Space Shooter Quick Setup",
            "This will create MainMenu, Game, and GameOver scenes in Assets/Scenes/ " +
            "and configure Build Settings.\n\nProceed?", "Yes", "Cancel"))
        {
            return;
        }

        // Ensure Scenes directory exists.
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // Create each scene.
        CreateScene("MainMenu", "MainMenuBootstrap", typeof(MainMenuSetup));
        CreateScene("Game", "GameBootstrap", typeof(GameSetup));
        CreateScene("GameOver", "GameOverBootstrap", typeof(GameOverSetup));

        // Configure Build Settings.
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameOver.unity", true),
        };

        // Open the MainMenu scene.
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");

        EditorUtility.DisplayDialog("Setup Complete",
            "All 3 scenes created and Build Settings configured!\n\n" +
            "• MainMenu.unity (index 0)\n" +
            "• Game.unity (index 1)\n" +
            "• GameOver.unity (index 2)\n\n" +
            "Press Play to test, or Build to create an .exe.",
            "OK");
    }

    private static void CreateScene(string sceneName, string bootstrapName, System.Type setupScript)
    {
        string path = $"Assets/Scenes/{sceneName}.unity";

        // Create a new empty scene.
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Remove the default directional light.
        foreach (GameObject go in scene.GetRootGameObjects())
        {
            if (go.name == "Directional Light")
            {
                Object.DestroyImmediate(go);
            }
        }

        // Create the bootstrap object.
        GameObject bootstrap = new GameObject(bootstrapName);
        bootstrap.AddComponent(setupScript);

        // Save the scene.
        EditorSceneManager.SaveScene(scene, path);

        Debug.Log($"[QuickSetup] Created scene: {path}");
    }
}
#endif
