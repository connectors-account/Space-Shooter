#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// EDITOR-ONLY helper that builds a Windows standalone executable.
/// Useful both from the menu and from the command line (headless CI builds).
///
/// Menu:        Tools -> Space Shooter -> Build Windows EXE
/// Command line:
///   "C:\Program Files\Unity\Hub\Editor\2021.3.30f1\Editor\Unity.exe" ^
///       -quit -batchmode -projectPath "PATH_TO_PROJECT" ^
///       -executeMethod BuildScript.BuildWindows ^
///       -logFile build.log
/// </summary>
public static class BuildScript
{
    // The scenes to include, in order. Index 0 is the first scene loaded.
    private static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/Game.unity"
    };

    [MenuItem("Tools/Space Shooter/Build Windows EXE")]
    public static void BuildWindows()
    {
        // Output location for the build.
        string outputPath = "Builds/Windows/SpaceShooter.exe";

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            Debug.Log($"Build succeeded: {report.summary.totalSize} bytes at {outputPath}");
        else
            Debug.LogError($"Build failed: {report.summary.result}");
    }
}
#endif
