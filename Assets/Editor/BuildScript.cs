#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-click Windows build helper. Adds a "Build" menu to the Unity Editor and
/// also supports headless/CI builds via -executeMethod BuildScript.BuildWindows.
/// </summary>
public static class BuildScript
{
    private const string BuildFolder = "Builds/Windows";
    private const string ExecutableName = "SpaceShooter.exe";

    [MenuItem("Build/Build Windows (x64)")]
    public static void BuildWindows()
    {
        // Collect scenes that are enabled in the Build Settings.
        string[] scenes = GetEnabledScenes();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes enabled in Build Settings. Add Assets/Scenes/MainScene.unity.");
            return;
        }

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = System.IO.Path.Combine(BuildFolder, ExecutableName),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded: {summary.totalSize} bytes at {options.locationPathName}");
        }
        else
        {
            Debug.LogError($"Build failed: {summary.result}");
        }
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                scenes.Add(scene.path);
            }
        }
        return scenes.ToArray();
    }
}
#endif
