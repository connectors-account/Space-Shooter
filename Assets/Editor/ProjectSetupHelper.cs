using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility to automate scene and project setup.
/// Access from the Unity menu: SpaceShooter > Setup Scene.
/// </summary>
public class ProjectSetupHelper : EditorWindow
{
    [MenuItem("SpaceShooter/Setup Scene")]
    public static void SetupScene()
    {
        // Create a new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Ensure the Player tag exists (it's built-in so it should)
        // Create the Bootstrap object
        GameObject bootstrap = new GameObject("Bootstrap");
        bootstrap.AddComponent<SceneBootstrap>();

        // Save the scene
        string scenePath = "Assets/Scenes/MainScene.unity";
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(
            Application.dataPath + "/../" + scenePath));
        EditorSceneManager.SaveScene(scene, scenePath);

        // Configure build settings
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        // Configure project settings for 2D
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.companyName = "IndieGameStudio";
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.SetApplicationIdentifier(
            BuildTargetGroup.Standalone, "com.indiegamestudio.spaceshooter");

        // Set resolution
        PlayerSettings.defaultIsNativeResolution = false;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

        // Physics 2D layer collision matrix is fine with defaults since we use triggers

        Debug.Log("Space Shooter scene setup complete! Press Play to test.");
        EditorUtility.DisplayDialog("Setup Complete",
            "Scene has been set up successfully!\n\n" +
            "Press Play to test the game.\n" +
            "Use SpaceShooter > Build Windows to create an .exe.", "OK");
    }

    [MenuItem("SpaceShooter/Build Windows")]
    public static void BuildWindows()
    {
        string buildPath = "Builds/Windows/SpaceShooter.exe";
        string dir = System.IO.Path.GetDirectoryName(buildPath);
        if (!System.IO.Directory.Exists(dir))
            System.IO.Directory.CreateDirectory(dir);

        BuildPlayerOptions options = new BuildPlayerOptions();
        options.scenes = new string[] { "Assets/Scenes/MainScene.unity" };
        options.locationPathName = buildPath;
        options.target = BuildTarget.StandaloneWindows64;
        options.options = BuildOptions.None;

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded! Output: {buildPath}");
            EditorUtility.DisplayDialog("Build Complete",
                $"Windows build created at:\n{System.IO.Path.GetFullPath(buildPath)}", "OK");
        }
        else
        {
            Debug.LogError($"Build failed: {report.summary.result}");
        }
    }
}
