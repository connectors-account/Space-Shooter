#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Editor utility that builds a standalone Windows (x64) .exe.
/// Use the menu item "Build > Build Windows (x64)" or run from the command
/// line via -executeMethod BuildScript.BuildWindows (see README for details).
/// </summary>
public static class BuildScript
{
    // Scenes included in the build, in order. Index 0 is the main menu.
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Game.unity"
    };

    [MenuItem("Build/Build Windows (x64)")]
    public static void BuildWindows()
    {
        // Output to <project>/Builds/Windows/SpaceShooter.exe
        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Windows");
        Directory.CreateDirectory(buildDir);
        string exePath = Path.Combine(buildDir, "SpaceShooter.exe");

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = exePath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"Build succeeded: {summary.totalSize} bytes at {exePath}");
        else
            Debug.LogError($"Build failed: {summary.result}");
    }
}
#endif
