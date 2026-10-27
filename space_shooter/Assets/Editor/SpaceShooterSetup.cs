using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MinimalSpaceShooter.Editor
{
    // This file is in Editor/: it is never compiled into the Windows player.
    public static class SpaceShooterSetup
    {
        private const string ScenePath = "Assets/Scenes/SpaceShooter.unity";

        [InitializeOnLoadMethod]
        private static void ScheduleSetup()
        {
            EditorApplication.delayCall += FirstImport;
        }

        private static void FirstImport()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath)) return;
            EnsureScene();
            Scene active = SceneManager.GetActiveScene();
            // Never discard unsaved work in the editor.
            if (!active.isDirty) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Space Shooter is ready. Open the Game tab and press Play. No wiring is required.");
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

        [MenuItem("Space Shooter/Open Game Scene")]
        public static void OpenGameScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureScene();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
