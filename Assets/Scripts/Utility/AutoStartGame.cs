using UnityEngine;

/// <summary>
/// AutoStartGame is placed in the GamePlay scene.
/// It waits one frame for all objects to initialize,
/// then tells the GameManager to start the game.
/// </summary>
public class AutoStartGame : MonoBehaviour
{
    void Start()
    {
        // Start the game after a brief delay to let everything initialize
        Invoke(nameof(BeginGame), 0.1f);
    }

    void BeginGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("AutoStartGame: GameManager.Instance is null! Make sure GameManager exists in the scene.");
        }
    }
}
