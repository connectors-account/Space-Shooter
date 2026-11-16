using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// All editor-only dependencies stay in an Editor folder and are excluded from builds.
[InitializeOnLoad]
public static class SpaceShooterSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    static SpaceShooterSetup()
    {
        EditorApplication.delayCall += FirstOpenSetup;
    }

    private static void FirstOpenSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += FirstOpenSetup;
            return;
        }
        bool createdScene = !File.Exists(ScenePath);
        EnsureAssets();
        if (createdScene && string.IsNullOrEmpty(SceneManager.GetActiveScene().path)
            && !SceneManager.GetActiveScene().isDirty)
            EditorSceneManager.OpenScene(ScenePath);
    }

    private static void EnsureAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        MakeMaterial("Player", new Color(0.1f, 0.85f, 1f));
        MakeMaterial("Enemy", new Color(1f, 0.2f, 0.3f));
        MakeMaterial("Bullet", new Color(1f, 0.9f, 0.3f));
        if (!File.Exists(ScenePath))
        {
            Scene previous = SceneManager.GetActiveScene();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.IO.IOException("Could not save " + ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    private static void MakeMaterial(string name, Color color)
    {
        string path = "Assets/Resources/" + name + ".mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        Shader shader = Shader.Find("Unlit/Color");
        if (shader == null) throw new System.InvalidOperationException("Built-in Unlit/Color shader not found. Use the built-in render pipeline.");
        Material material = new Material(shader) { name = name, color = color };
        // Saved material references keep this shader in the standalone player build.
        AssetDatabase.CreateAsset(material, path);
    }

    [MenuItem("Space Shooter/Setup / Open Main")]
    public static void OpenMain()
    {
        if (EditorApplication.isPlaying) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EnsureAssets();
        EditorSceneManager.OpenScene(ScenePath);
    }

    [MenuItem("Space Shooter/Build Windows x86_64")]
    public static void BuildWindows()
    {
        if (EditorApplication.isPlaying)
            throw new System.InvalidOperationException("Exit Play mode before building.");
        EnsureAssets();
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            throw new System.InvalidOperationException("Windows build support is unavailable. Open this project in the Windows Unity Editor with Windows build support installed.");
        PlayerSettings.companyName = "SpaceShooter";
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
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
            throw new BuildFailedException("Windows build failed: " + report.summary.result + ". See the Unity Console.");
        Debug.Log("Windows x86_64 build complete: " + Path.GetFullPath("Builds/Windows/SpaceShooter.exe"));
    }
}
