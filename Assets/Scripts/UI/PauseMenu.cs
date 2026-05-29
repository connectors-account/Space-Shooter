using UnityEngine;

/// <summary>
/// Pause menu is handled via GameManager.TogglePause() and UIManager.ShowPauseMenu().
/// This script provides an alternative standalone pause handler if needed.
/// Attach to any persistent GameObject in the Game scene.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // Pause is triggered by ESC key in GameManager.Update().
    // This component exists as a convenience if you want to separate
    // pause logic from GameManager in the future.

    void Update()
    {
        // Handled in GameManager — kept here for scene organization clarity.
    }

    /// <summary>
    /// Call from a UI Resume button via UnityEvent or code.
    /// </summary>
    public void Resume()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.TogglePause();
    }

    /// <summary>
    /// Call from a UI Main Menu button via UnityEvent or code.
    /// </summary>
    public void GoToMainMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.GoToMainMenu();
    }
}
