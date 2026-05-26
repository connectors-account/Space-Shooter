#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

/// <summary>
/// Automated build script for creating Windows x64 executables.
/// Access via the Unity menu: Build → Build Windows x64
/// Also supports command-line invocation for CI/CD pipelines.
/// </summary>
public class BuildScript
{
    private const string BuildPath = "Builds/Windows/SpaceShooter.exe";

    [MenuItem("Build/Build Windows x64 (Development)")]
    public static void BuildWindowsDev()
    {
        Build(BuildOptions.Development | BuildOptions.AllowDebugging);
    }

    [MenuItem("Build/Build Windows x64 (Release)")]
    public static void BuildWindowsRelease()
    {
        Build(BuildOptions.None);
    }

    /// <summary>
    /// Entry point for command-line builds.
    /// Usage: Unity.exe -quit -batchmode -executeMethod BuildScript.CommandLineBuild
    /// </summary>
    public static void CommandLineBuild()
    {
        Build(BuildOptions.None);
    }

    private static void Build(BuildOptions options)
    {
        // Ensure build directory exists
        string dir = Path.GetDirectoryName(BuildPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // Collect all enabled scenes
        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("BuildScript: No scenes found in Build Settings. " +
                           "Add scenes via File → Build Settings → Add Open Scenes.");
            return;
        }

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = BuildPath,
            target = BuildTarget.StandaloneWindows64,
            options = options
        };

        Debug.Log($"BuildScript: Starting build → {BuildPath}");
        Debug.Log($"BuildScript: Scenes: {string.Join(", ", scenes)}");
        Debug.Log($"BuildScript: Options: {options}");

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        switch (summary.result)
        {
            case BuildResult.Succeeded:
                Debug.Log($"✅ Build SUCCEEDED" +
                          $"\n   Output: {summary.outputPath}" +
                          $"\n   Size: {summary.totalSize / (1024 * 1024):F1} MB" +
                          $"\n   Time: {summary.totalTime.TotalSeconds:F1}s" +
                          $"\n   Warnings: {summary.totalWarnings}" +
                          $"\n   Errors: {summary.totalErrors}");
                break;

            case BuildResult.Failed:
                Debug.LogError($"❌ Build FAILED with {summary.totalErrors} error(s).");
                break;

            case BuildResult.Cancelled:
                Debug.LogWarning("⚠️ Build was cancelled.");
                break;

            default:
                Debug.LogWarning($"Build result: {summary.result}");
                break;
        }
    }

    private static string[] GetEnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }

        // Fallback: if no scenes in build settings, try to find GameScene
        if (scenes.Count == 0)
        {
            string fallback = "Assets/Scenes/GameScene.unity";
            if (File.Exists(fallback))
            {
                scenes.Add(fallback);
                Debug.LogWarning($"BuildScript: No scenes in Build Settings. " +
                                 $"Using fallback: {fallback}");
            }
        }

        return scenes.ToArray();
    }
}
#endif
