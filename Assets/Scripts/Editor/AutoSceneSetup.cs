#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically create and configure the game scenes.
/// Run from menu: Space Shooter > Setup Scenes
/// </summary>
public class AutoSceneSetup : MonoBehaviour
{
    [MenuItem("Space Shooter/Setup All Scenes")]
    public static void SetupAllScenes()
    {
        CreateMainMenuScene();
        CreateGameScene();
        SetupBuildSettings();
        SetupTags();
        Debug.Log("Space Shooter: All scenes created and configured!");
    }

    [MenuItem("Space Shooter/Setup Tags and Layers")]
    public static void SetupTags()
    {
        // Tags and layers are defined in TagManager.asset
        // This is a reminder to verify they're set correctly
        Debug.Log("Tags and Layers should be configured via ProjectSettings/TagManager.asset");
        Debug.Log("Required Tags: PlayerBullet, EnemyBullet, PowerUp, Enemy, Player, Background");
        Debug.Log("Required Layers: Player(8), Enemy(9), PlayerBullet(10), EnemyBullet(11), PowerUp(12), Background(13)");
        Debug.Log("Required Sorting Layers: Background, Midground, Foreground, Player, Bullets, UI");
    }

    private static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create setup object
        GameObject setupObj = new GameObject("MainMenuSetup");
        setupObj.AddComponent<MainMenuSetup>();

        // Save scene
        string path = "Assets/Scenes/MainMenu.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Created MainMenu scene at: " + path);
    }

    private static void CreateGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create setup object
        GameObject setupObj = new GameObject("GameSceneSetup");
        setupObj.AddComponent<GameSceneSetup>();

        // Save scene
        string path = "Assets/Scenes/GameScene.unity";
        EditorSceneManager.SaveScene(scene, path);
        Debug.Log("Created GameScene at: " + path);
    }

    private static void SetupBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("Build settings configured with MainMenu and GameScene");
    }
}
#endif
