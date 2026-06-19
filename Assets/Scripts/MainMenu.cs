using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main-menu scene. Wire the Start and Quit buttons' OnClick
/// events to these public methods in the Inspector.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Tooltip("Name of the gameplay scene to load. Must be added to Build Settings.")]
    public string gameplaySceneName = "Game";

    /// <summary>Start button: load the gameplay scene.</summary>
    public void OnStartButton()
    {
        // Loading by name is robust as long as the scene is in Build Settings.
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>Quit button: exit the application (no effect in the editor).</summary>
    public void OnQuitButton()
    {
        Debug.Log("Quit requested.");
        Application.Quit();

#if UNITY_EDITOR
        // Stop play-mode when testing inside the editor.
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
