using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SpaceShooterSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    private static void Configure()
    {
        PlayerSettings.companyName = "Space Shooter";
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = false;
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Space Shooter/Open Game Scene")]
    public static void OpenGameScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Configure();
        EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Space Shooter/Build Windows x64")]
    public static void BuildWindows()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Stop Play Mode", "Exit Play Mode before building.", "OK");
            return;
        }
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
        {
            EditorUtility.DisplayDialog("Windows module required",
                "In Unity Hub, add Windows Build Support for this editor installation, then try again.", "OK");
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Configure();
        string directory = Path.GetFullPath("Builds/Windows");
        Directory.CreateDirectory(directory);
        string path = EditorUtility.SaveFilePanel("Build Windows x64", directory, "SpaceShooter", "exe");
        if (string.IsNullOrEmpty(path)) return;
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = path,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.RevealInFinder(path);
            EditorUtility.DisplayDialog("Build complete",
                "Share the entire output folder, not just the .exe file.", "OK");
        }
        else
        {
            Debug.LogError("Windows build result: " + report.summary.result);
            EditorUtility.DisplayDialog("Build did not succeed", "See the Unity Console for details.", "OK");
        }
    }
}
