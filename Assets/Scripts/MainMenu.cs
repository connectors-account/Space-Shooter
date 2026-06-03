using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu scene.
///   - Start button loads the gameplay scene.
///   - Quit button exits the application (or stops play mode in the editor).
/// Attach this to a GameObject in the MainMenu scene and wire the buttons'
/// OnClick events to the public methods below.
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Name of the gameplay scene to load. Must be added to Build Settings.")]
    public string gameSceneName = "Game";

    /// <summary>
    /// Loads the main gameplay scene. Hook this to the Start button's OnClick.
    /// </summary>
    public void StartGame()
    {
        // Ensure time is running normally (in case it was paused elsewhere).
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Quits the application. Hook this to the Quit button's OnClick.
    /// In the editor this stops play mode; in a build it closes the game.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Quit requested.");

#if UNITY_EDITOR
        // When testing inside the Unity Editor, stop play mode.
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // In a standalone build, close the application.
        Application.Quit();
#endif
    }
}
