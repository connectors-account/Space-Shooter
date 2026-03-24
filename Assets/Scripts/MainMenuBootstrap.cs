using UnityEngine;

/// <summary>
/// MainMenuBootstrap handles initialization when the MainMenu scene loads.
/// It plays the menu music and ensures the GameManager singleton exists.
/// Attach this to an empty GameObject in the MainMenu scene.
/// </summary>
public class MainMenuBootstrap : MonoBehaviour
{
    [Header("Prefabs to ensure exist")]
    [SerializeField] private GameObject gameManagerPrefab;
    [SerializeField] private GameObject audioManagerPrefab;

    private void Awake()
    {
        // Ensure GameManager exists
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }

        // Ensure AudioManager exists
        if (AudioManager.Instance == null && audioManagerPrefab != null)
        {
            Instantiate(audioManagerPrefab);
        }
    }

    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }
}
