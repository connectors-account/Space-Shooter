#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpaceShooter.EditorTools
{
    public static class SceneSetupEditor
    {
        private const string MainScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Tools/Space Shooter/Create Main Scene")]
        public static void CreateMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("SceneRoot");
            root.transform.position = Vector3.zero;

            EditorSceneManager.SaveScene(scene, MainScenePath);

            EditorBuildSettingsScene[] scenes =
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            Debug.Log("Main scene created at Assets/Scenes/Main.unity and added to Build Settings.");
        }
    }
}
#endif
