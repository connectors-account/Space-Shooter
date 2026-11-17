using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace SpaceShooter.Editor
{
    public static class WindowsBuild
    {
        [MenuItem("Space Shooter/Build Windows x86-64")]
        public static void BuildWindows()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone,
                BuildTarget.StandaloneWindows64))
                throw new BuildFailedException("Windows build support is missing. Add it for this Editor in Unity Hub.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                throw new BuildFailedException("Select Windows / Intel 64-bit in File > Build Profiles and Switch Platform, then retry.");
            const string folder = "Builds/Windows";
            Directory.CreateDirectory(folder);
            PlayerSettings.companyName = "Independent";
            PlayerSettings.productName = "Space Shooter";
            PlayerSettings.defaultScreenWidth = 960;
            PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Main.unity" },
                locationPathName = folder + "/SpaceShooter.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Windows build failed. See the Unity Console for details.");
            Debug.Log("Windows x86-64 build succeeded: " + Path.GetFullPath(folder));
        }
    }
}
