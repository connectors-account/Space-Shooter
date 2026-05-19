using UnityEngine;

/// <summary>
/// Bootstrapper that initializes persistent managers on first load.
/// Place this on a GameObject in the MainMenu scene.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("Manager Prefabs (optional - auto-created if null)")]
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject scoreManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;

    private void Awake()
    {
        // Create GameManager if not present
        if (GameManager.Instance == null)
        {
            GameObject gm = new GameObject("GameManager");
            gm.AddComponent<GameManager>();
        }

        // Create ScoreManager if not present
        if (ScoreManager.Instance == null)
        {
            GameObject sm = new GameObject("ScoreManager");
            sm.AddComponent<ScoreManager>();
        }

        // Create AudioManager if not present
        if (AudioManager.Instance == null)
        {
            GameObject am = new GameObject("AudioManager");
            am.AddComponent<AudioManager>();
        }
    }
}
