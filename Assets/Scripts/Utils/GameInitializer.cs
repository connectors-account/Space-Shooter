using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Initializes required game systems. Place in the first scene that loads.
/// Creates GameManager and AudioManager if they don't exist.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [SerializeField] private bool isGameScene = false;

    private void Awake()
    {
        // Create GameManager if it doesn't exist
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        // Create AudioManager if it doesn't exist
        if (AudioManager.Instance == null)
        {
            GameObject amObj = new GameObject("AudioManager");
            amObj.AddComponent<AudioManager>();
        }
    }

    private void Start()
    {
        if (isGameScene)
        {
            AudioManager.Instance?.PlayGameMusic();
        }
        else
        {
            AudioManager.Instance?.PlayMenuMusic();
        }
    }
}
