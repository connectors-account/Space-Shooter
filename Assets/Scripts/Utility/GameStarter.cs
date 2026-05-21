using UnityEngine;

/// <summary>
/// GameStarter - Auto-starts the game when the Game scene loads.
/// Attach to the GameManager GameObject alongside GameManager.cs.
/// This ensures the game begins as soon as the scene transitions from the main menu.
/// </summary>
public class GameStarter : MonoBehaviour
{
    private void Start()
    {
        // Wait a frame to ensure all managers are initialized
        Invoke(nameof(BeginGame), 0.1f);
    }

    private void BeginGame()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
        {
            GameManager.Instance.StartGame();
        }
    }
}
