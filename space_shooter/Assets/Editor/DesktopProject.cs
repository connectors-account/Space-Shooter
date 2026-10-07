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
    public static class DesktopProject
    {
        private const string MainScene = "Assets/Scenes/Main.unity";

        static DesktopProject()
        {
            EditorApplication.delayCall += OpenInitialScene;
        }

        private static void OpenInitialScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            Scene scene = SceneManager.GetActiveScene();
            // Only replace the empty initial editor scene, never a user's saved/dirty scene.
            if (SceneManager.sceneCount == 1 && string.IsNullOrEmpty(scene.path) && !scene.isDirty)
                EditorSceneManager.OpenScene(MainScene);
        }

        [MenuItem("Space Shooter/Build Windows x64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new BuildFailedException("Exit Play mode before building.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Install Windows Build Support for this Unity Editor through Unity Hub.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Unity could not switch to Windows x64.");

            PlayerSettings.companyName = "Space Shooter";
            PlayerSettings.productName = "Space Shooter";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            Directory.CreateDirectory("Builds/Windows");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MainScene },
                locationPathName = "Builds/Windows/SpaceShooter.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows build failed. See the Unity Console for details.");
            Debug.Log("Windows x64 build created: " + Path.GetFullPath("Builds/Windows/SpaceShooter.exe"));
        }
    }
}
