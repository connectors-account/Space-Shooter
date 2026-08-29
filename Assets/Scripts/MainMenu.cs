using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main-menu button handlers.
/// Attach to any GameObject in the MainMenu scene.
/// Wire each Button's OnClick() to these methods in the Inspector.
/// </summary>
public class MainMenu : MonoBehaviour
{
    // Called by the PLAY button
    public void OnPlayClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadGame();
        else
            SceneManager.LoadScene("Game");
    }

    // Called by the QUIT button
    public void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitGame();
        else
            Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
