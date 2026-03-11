using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ensures all singleton managers are initialized on game start.
/// Place this in the first scene (MainMenu) or use RuntimeInitializeOnLoadMethod.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Manager Prefabs")]
    public GameObject gameManagerPrefab;
    public GameObject audioManagerPrefab;

    private static bool isInitialized = false;

    private void Awake()
    {
        if (isInitialized) return;

        InitializeManagers();
        isInitialized = true;
    }

    private void InitializeManagers()
    {
        // Create GameManager if not exists
        if (GameManager.Instance == null)
        {
            if (gameManagerPrefab != null)
            {
                Instantiate(gameManagerPrefab);
            }
            else
            {
                GameObject gmObj = new GameObject("GameManager");
                gmObj.AddComponent<GameManager>();
                gmObj.AddComponent<ScoreManager>();
            }
        }

        // Create AudioManager if not exists
        if (AudioManager.Instance == null)
        {
            if (audioManagerPrefab != null)
            {
                Instantiate(audioManagerPrefab);
            }
            else
            {
                GameObject amObj = new GameObject("AudioManager");
                amObj.AddComponent<AudioManager>();
            }
        }
    }

    /// <summary>
    /// Alternative: Initialize before any scene loads
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        // Uncomment to auto-create managers:
        // if (GameManager.Instance == null)
        // {
        //     GameObject gmObj = new GameObject("GameManager");
        //     gmObj.AddComponent<GameManager>();
        //     gmObj.AddComponent<ScoreManager>();
        //     DontDestroyOnLoad(gmObj);
        // }
    }
}
