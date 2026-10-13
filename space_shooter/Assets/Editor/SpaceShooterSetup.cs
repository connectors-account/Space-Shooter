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
    [InitializeOnLoad]
    public static class SpaceShooterSetup
    {
        private const string ScenePath = "Assets/Scenes/Game.unity";

        static SpaceShooterSetup()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        // The empty saved scene is intentional: SpaceShooterGame bootstraps at runtime.
        // Create it additively, so importing this project never discards an unsaved scene.
        private static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory("Assets/Scenes");
                AssetDatabase.Refresh();
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new IOException("Could not save the generated Game scene.");
                EditorSceneManager.CloseScene(scene, true);
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        [MenuItem("Space Shooter/Open Game Scene")]
        public static void OpenGame()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureScene();
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Space Shooter/Build Windows x64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play mode before building.");
            EnsureScene();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Install Windows Build Support for this Unity Editor using Unity Hub.");
            PlayerSettings.companyName = "Space Shooter";
            PlayerSettings.productName = "Space Shooter";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 600;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
            // Mono keeps this tiny desktop project independent of Visual Studio / IL2CPP tooling.
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Directory.CreateDirectory("Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/SpaceShooter.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result + ". See the Unity Console.");
            Debug.Log("Windows build saved to Builds/Windows/SpaceShooter.exe. Distribute the entire Windows folder.");
        }
    }
}
