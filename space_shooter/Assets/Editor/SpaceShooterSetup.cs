using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

// Generates the actual scene on first import. Generated assets are saved to disk
// and the scene is enabled in the shared build scene list.
[InitializeOnLoad]
public static class SpaceShooterSetup
{
    private const string ScenePath = "Assets/Scenes/Game.unity";
    private const string MaterialPath = "Assets/Materials/Shapes.mat";

    static SpaceShooterSetup()
    {
        EditorApplication.delayCall += CreateOnFirstImport;
    }

    private static void CreateOnFirstImport()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += CreateOnFirstImport;
            return;
        }
        if (!File.Exists(ScenePath)) OpenOrCreateScene();
    }

    [MenuItem("Space Shooter/Open / Create Game Scene")]
    public static void OpenOrCreateScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        PlayerSettings.companyName = "Space Shooter";
        PlayerSettings.productName = "Space Shooter";
        PlayerSettings.defaultScreenWidth = 960;
        PlayerSettings.defaultScreenHeight = 540;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = false;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);

        if (!File.Exists(ScenePath))
        {
            Directory.CreateDirectory("Assets/Scenes");
            Directory.CreateDirectory("Assets/Materials");
            AssetDatabase.Refresh();
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null)
                    throw new System.InvalidOperationException("Built-in Sprites/Default shader was not found.");
                material = new Material(shader) { name = "Shapes" };
                AssetDatabase.CreateAsset(material, MaterialPath);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Space Shooter");
            var game = root.AddComponent<SpaceShooter>();
            var serializedGame = new SerializedObject(game);
            serializedGame.FindProperty("shapeMaterial").objectReferenceValue = material;
            serializedGame.ApplyModifiedPropertiesWithoutUndo();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new System.IO.IOException("Could not save " + ScenePath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Space Shooter is ready for Play. Click the Game view to give it keyboard focus.");
    }
}
