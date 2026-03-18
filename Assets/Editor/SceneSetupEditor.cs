using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor menu tool to auto-set up the game scene with a single click.
/// Access via: Tools > Space Shooter > Setup Scene
/// </summary>
public class SceneSetupEditor : Editor
{
    [MenuItem("Tools/Space Shooter/Setup Scene (One-Click)")]
    public static void SetupScene()
    {
        // Create or open a new scene
        if (EditorUtility.DisplayDialog("Setup Space Shooter Scene",
            "This will set up the game scene in the currently open scene. Continue?",
            "Yes", "Cancel"))
        {
            CreateGameScene();
        }
    }

    [MenuItem("Tools/Space Shooter/Create New Scene and Setup")]
    public static void CreateNewSceneAndSetup()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        CreateGameScene();

        // Save the scene
        string scenePath = "Assets/Scenes/GameScene.unity";
        if (!System.IO.Directory.Exists("Assets/Scenes"))
            System.IO.Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);
        Debug.Log("Scene saved to " + scenePath);
    }

    private static void CreateGameScene()
    {
        // Set up camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
            cam.transform.position = new Vector3(0, 0, -10);
        }

        // Create GameBootstrapper - it handles everything at runtime
        GameObject bootstrapper = new GameObject("[GameBootstrapper]");
        bootstrapper.AddComponent<GameBootstrapper>();

        Debug.Log("=== Space Shooter Scene Setup Complete ===");
        Debug.Log("The GameBootstrapper will create all game objects at runtime.");
        Debug.Log("Just press Play to start the game!");
        Debug.Log("Controls: WASD/Arrow Keys to move, Space to shoot, ESC to pause");

        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    [MenuItem("Tools/Space Shooter/Configure Build Settings")]
    public static void ConfigureBuildSettings()
    {
        // Set build target
        EditorUserBuildSettings.SwitchActiveBuildTarget(
            BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        // Set player settings
        PlayerSettings.companyName = "SpaceShooterDev";
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.runInBackground = false;

        // Set resolution dialog
        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;

        Debug.Log("Build settings configured for Windows Standalone (64-bit)");
        Debug.Log("Product Name: Space Shooter");
        Debug.Log("Resolution: 1920x1080");
    }
}
