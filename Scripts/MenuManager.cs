using UnityEngine;

/// <summary>
/// Handles UI button actions for start menu, restart, and quit.
/// Attach this to a MenuManager GameObject in the scene.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        GameManager.Instance?.StartGame();
    }

    public void OnRestartButtonPressed()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnQuitButtonPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
