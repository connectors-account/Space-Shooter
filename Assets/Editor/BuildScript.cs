#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    /// <summary>
    /// Editor-only helpers for building the game from the menu or the command line.
    /// Invoke from CLI with:
    /// <c>Unity.exe -quit -batchmode -projectPath &lt;path&gt; -executeMethod SpaceShooter.EditorTools.BuildScript.BuildWindows</c>
    /// </summary>
    public static class BuildScript
    {
        private static readonly string[] Scenes =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GamePlay.unity"
        };

        /// <summary>Builds a 64-bit Windows standalone player into <c>Build/Windows/SpaceShooter.exe</c>.</summary>
        [MenuItem("SpaceShooter/Build Windows (x86_64)")]
        public static void BuildWindows()
        {
            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "Build", "Windows");
            Directory.CreateDirectory(outputDir);
            string exePath = Path.Combine(outputDir, "SpaceShooter.exe");

            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes at {exePath}");
            }
            else
            {
                Debug.LogError($"Build failed: {summary.result}");
            }
        }
    }
}
#endif
