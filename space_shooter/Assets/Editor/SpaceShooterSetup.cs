using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Unity runs this after importing the project. No manual scene assembly needed.
[InitializeOnLoad]
public static class SpaceShooterSetup
{
    private const string ScenePath = "Assets/Scenes/Main.unity";

    static SpaceShooterSetup()
    {
        EditorApplication.delayCall += EnsureScene;
    }

    private static void EnsureScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += EnsureScene;
            return;
        }
        if (File.Exists(ScenePath)) return;

        Directory.CreateDirectory("Assets/Scenes");
        AssetDatabase.Refresh();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        GameObject controller = new GameObject("Space Shooter");
        SceneManager.MoveGameObjectToScene(controller, scene);
        controller.AddComponent<SpaceShooter>();
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            Debug.LogError("Could not save " + ScenePath + ". Check project folder permissions.");
            return;
        }
        EditorSceneManager.CloseScene(scene, true);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.companyName = "Space Shooter";
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 600;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        Debug.Log("Space Shooter is ready. Press Play, or use Space Shooter > Open Game Scene.");
    }

    [MenuItem("Space Shooter/Open Game Scene")]
    public static void OpenScene()
    {
        EnsureScene();
        if (File.Exists(ScenePath) && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            EditorSceneManager.OpenScene(ScenePath);
    }
}
