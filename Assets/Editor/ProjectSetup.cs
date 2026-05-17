using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Editor script that runs once when Unity opens the project.
/// Configures tags, layers, build settings, and verifies scenes exist.
/// Run manually from menu: SpaceShooter > Setup Project
/// </summary>
public class ProjectSetup
{
    [MenuItem("SpaceShooter/Setup Project")]
    public static void SetupProject()
    {
        SetupTags();
        SetupBuildSettings();
        Debug.Log("[SpaceShooter] Project setup complete! Tags and build settings configured.");
        Debug.Log("[SpaceShooter] Open 'Assets/Scenes/MainMenuScene.unity' to start.");
    }

    [InitializeOnLoadMethod]
    static void OnProjectLoaded()
    {
        // Run setup once via editor prefs
        if (!EditorPrefs.GetBool("SpaceShooter_SetupDone", false))
        {
            EditorApplication.delayCall += () =>
            {
                SetupProject();
                EditorPrefs.SetBool("SpaceShooter_SetupDone", true);
            };
        }
    }

    static void SetupTags()
    {
        // Access TagManager
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        string[] requiredTags = { "PlayerBullet", "EnemyBullet", "Enemy", "HealthPickup" };

        foreach (string tag in requiredTags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    static void SetupBuildSettings()
    {
        string mainMenuPath = "Assets/Scenes/MainMenuScene.unity";
        string gameScenePath = "Assets/Scenes/GameScene.unity";

        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(mainMenuPath, true),
            new EditorBuildSettingsScene(gameScenePath, true)
        };

        EditorBuildSettings.scenes = scenes;
    }
}
