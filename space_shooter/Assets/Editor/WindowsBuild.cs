using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SpaceShooter.Editor
{
    public static class WindowsBuild
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Space Shooter/Build Windows x86_64")]
        public static void Build()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                EditorUtility.DisplayDialog("Windows Build Support required",
                    "Install Windows Build Support (Mono) for this Unity version using Unity Hub, then reopen the project.", "OK");
                return;
            }

            string directory = EditorUtility.SaveFolderPanel("Choose Windows build folder", "", "SpaceShooterBuild");
            if (string.IsNullOrEmpty(directory)) return;

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = Path.Combine(directory, "SpaceShooter.exe"),
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Windows x86_64 build created at " + options.locationPathName);
                EditorUtility.RevealInFinder(options.locationPathName);
            }
            else
            {
                Debug.LogError("Windows build did not succeed: " + report.summary.result
                    + ". See the Unity Console for details.");
            }
        }
    }
}
