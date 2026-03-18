using UnityEditor;
using UnityEngine;

/// <summary>
/// Quick build utility to compile the game to a Windows executable.
/// Access via: Tools > Space Shooter > Build Windows Executable
/// </summary>
public class QuickBuild : Editor
{
    [MenuItem("Tools/Space Shooter/Build Windows Executable")]
    public static void BuildGame()
    {
        // Ensure scenes list has our scene
        string[] scenes = new string[0];
        var currentScenes = EditorBuildSettings.scenes;

        if (currentScenes.Length == 0)
        {
            // Try to find GameScene
            string scenePath = "Assets/Scenes/GameScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                scenes = new string[] { scenePath };
            }
            else
            {
                // Use the currently open scene
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                if (!string.IsNullOrEmpty(currentScene))
                {
                    scenes = new string[] { currentScene };
                }
                else
                {
                    EditorUtility.DisplayDialog("Build Error",
                        "No scenes found. Please save your scene first (Ctrl+S) and try again.",
                        "OK");
                    return;
                }
            }
        }
        else
        {
            scenes = new string[currentScenes.Length];
            for (int i = 0; i < currentScenes.Length; i++)
                scenes[i] = currentScenes[i].path;
        }

        string buildPath = EditorUtility.SaveFolderPanel("Choose Build Location", "Builds", "");
        if (string.IsNullOrEmpty(buildPath)) return;

        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath + "/SpaceShooter.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(buildOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded! Output: {buildPath}/SpaceShooter.exe");
            Debug.Log($"Build size: {report.summary.totalSize / (1024 * 1024)} MB");
            EditorUtility.DisplayDialog("Build Complete",
                $"Build succeeded!\nOutput: {buildPath}/SpaceShooter.exe",
                "OK");
        }
        else
        {
            Debug.LogError($"Build failed: {report.summary.result}");
            EditorUtility.DisplayDialog("Build Failed",
                $"Build failed with result: {report.summary.result}\nCheck Console for details.",
                "OK");
        }
    }
}
