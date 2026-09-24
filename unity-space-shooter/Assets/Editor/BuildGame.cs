using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Starward;

namespace StarwardEditor
{
    public static class BuildGame
    {
        [MenuItem("Starward/Validate Generated Project")]
        public static void ValidateProject()
        {
            if (!File.Exists(ProjectSetup.ScenePath)) throw new FileNotFoundException("Run Starward > Create or Rebuild Game first.");
            string[] prefabNames = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "RepairPickup", "WeaponPickup", "Burst" };
            foreach (string name in prefabNames)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectSetup.PrefabsPath + name + ".prefab");
                Need(prefab != null, "Missing prefab: " + name);
                ValidateObjects(prefab);
                if (name == "Burst") continue;
                Need(prefab.GetComponent<Rigidbody2D>() != null, name + " needs a Rigidbody2D");
                Need(prefab.GetComponent<Collider2D>() != null && prefab.GetComponent<Collider2D>().isTrigger, name + " needs a trigger collider");
                Need(prefab.GetComponent<SpriteRenderer>().sprite != null, name + " missing sprite");
            }
            Scene scene = SceneManager.GetSceneByPath(ProjectSetup.ScenePath);
            bool temporary = !scene.isLoaded;
            if (temporary) scene = EditorSceneManager.OpenScene(ProjectSetup.ScenePath, OpenSceneMode.Additive);
            try
            {
                GameManager game = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    ValidateObjects(root);
                    GameManager candidate = root.GetComponent<GameManager>();
                    if (candidate != null) game = candidate;
                }
                Need(game != null, "Missing GameManager");
                Need(game.playerPrefab && game.enemyPrefab && game.playerBulletPrefab && game.enemyBulletPrefab && game.repairPrefab && game.weaponPrefab && game.burstPrefab, "Missing gameplay prefab references");
                Need(game.entities && game.director && game.audioBank, "Missing scene service references");
                Need(game.audioBank.clips != null && game.audioBank.clips.Length == 8, "Expected eight audio clips");
                foreach (AudioClip clip in game.audioBank.clips) Need(clip != null && clip.length > 0, "Missing or empty audio clip");
                Need(LayerMask.NameToLayer("Player") == 8 && LayerMask.NameToLayer("Enemy") == 9, "Unexpected collision layer configuration");
            }
            finally { if (temporary) EditorSceneManager.CloseScene(scene, true); }
            Debug.Log("Starward reference validation passed. This validates assets, not gameplay.");
        }
        static void ValidateObjects(GameObject root)
        {
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                Need(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject) == 0, "Missing script on " + node.name);
        }
        static void Need(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

        [MenuItem("Starward/Build Windows x86_64")]
        public static void BuildWindows()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play mode before building.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
                throw new InvalidOperationException("Install Windows Build Support for this Unity editor through Unity Hub.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
                throw new InvalidOperationException("File > Build Profiles: select Windows, Intel 64-bit architecture, then Switch Platform. Batch mode: pass -buildTarget Win64.");
            if (!File.Exists(ProjectSetup.ScenePath)) ProjectSetup.Generate();
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ValidateProject();
            string destination = "Builds/Windows/Starward.exe";
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ProjectSetup.ScenePath },
                locationPathName = destination,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows build failed: " + report.summary.result + ". Read the Unity Console/build log.");
            Debug.Log("Windows x86_64 build succeeded: " + Path.GetFullPath(destination) + ". Distribute the whole Builds/Windows folder.");
        }
    }
}
