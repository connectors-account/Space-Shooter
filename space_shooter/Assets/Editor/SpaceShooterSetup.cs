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
        private const string ScenePath = "Assets/Scenes/Main.unity";

        static SpaceShooterSetup()
        {
            EditorApplication.delayCall += FirstOpen;
        }

        private static void FirstOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
            if (!File.Exists(ScenePath))
            {
                EnsureScene();
                // Never discard an existing user's unsaved scene.
                Scene active = SceneManager.GetActiveScene();
                if (!Application.isBatchMode && !active.isDirty && string.IsNullOrEmpty(active.path))
                    EditorSceneManager.OpenScene(ScenePath);
                Debug.Log("Space Shooter is ready. Use Space Shooter > Play Game.");
            }
        }

        private static void EnsureScene()
        {
            if (!File.Exists(ScenePath))
            {
                Directory.CreateDirectory("Assets/Scenes");
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                    throw new IOException("Could not save " + ScenePath);
                EditorSceneManager.CloseScene(scene, true);
                AssetDatabase.Refresh();
            }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        }

        [MenuItem("Space Shooter/Play Game")]
        public static void PlayGame()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureScene();
            EditorSceneManager.OpenScene(ScenePath);
            EditorApplication.isPlaying = true;
        }

        // Also callable with -executeMethod SpaceShooter.Editor.SpaceShooterSetup.BuildWindows.
        [MenuItem("Space Shooter/Build Windows x64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Exit Play mode before building.");
            EnsureScene();
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Windows build support is unavailable. Install the matching Windows Editor in Unity Hub.");

            PlayerSettings.companyName = "SpaceShooter";
            PlayerSettings.productName = "Space Shooter";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.runInBackground = false;
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
                throw new BuildFailedException("Windows build failed: " + report.summary.result + ". Check the Console.");
            Debug.Log("Windows build succeeded: " + Path.GetFullPath("Builds/Windows/SpaceShooter.exe"));
        }
    }
}
