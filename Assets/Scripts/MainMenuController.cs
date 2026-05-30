using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// MainMenuController – Handles the main menu buttons.
/// Attach to the Canvas in the MainMenu scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);
    }

    void OnStartGame()
    {
        SceneManager.LoadScene("GamePlay");
    }

    void OnQuitGame()
    {
        // In the editor this won't close, but in a build it will
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
