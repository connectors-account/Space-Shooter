using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter.Editor
{
    public static class SpaceShooterSetup
    {
        public const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Space Shooter/Open Game Scene")]
        public static void OpenScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureScene();
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Space Shooter/Configure Project")]
        public static void ConfigureProject()
        {
            EnsureScene();
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            PlayerSettings.companyName = "Space Shooter";
            PlayerSettings.productName = "Space Shooter";
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 800;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono);
            AssetDatabase.SaveAssets();
            Debug.Log("Space Shooter configured. Open the game scene and press Play.");
        }

        [MenuItem("Space Shooter/Build Windows x86_64")]
        public static void BuildWindows()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                EditorUtility.DisplayDialog("Windows build support required",
                    "Install the Windows Build Support module for this Unity editor in Unity Hub, then retry.", "OK");
                return;
            }
            string folder = EditorUtility.SaveFolderPanel("Choose Windows build output folder", "", "SpaceShooterBuild");
            if (string.IsNullOrEmpty(folder)) return;
            BuildTo(Path.Combine(folder, "SpaceShooter.exe"));
        }

        // Optional batch entry point: -executeMethod SpaceShooter.Editor.SpaceShooterSetup.BuildWindowsBatch
        // Defaults to Builds/Windows/SpaceShooter.exe; override with -buildOutput <full exe path>.
        public static void BuildWindowsBatch()
        {
            string output = Path.GetFullPath("Builds/Windows/SpaceShooter.exe");
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i] == "-buildOutput") output = Path.GetFullPath(args[i + 1]);
            BuildTo(output);
        }

        private static void BuildTo(string output)
        {
            ConfigureProject();
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows build failed: " + report.summary.result);
            Debug.Log("Windows build created: " + output);
        }

        private static void EnsureScene()
        {
            if (File.Exists(ScenePath)) return;
            Directory.CreateDirectory("Assets/Scenes");
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.Refresh();
        }
    }
}
