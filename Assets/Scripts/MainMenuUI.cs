using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu UI (Start Game and Quit buttons).
/// Attach this to a "MainMenuUI" GameObject in the MainMenu scene.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button startButton;
    public Button quitButton;

    [Header("Optional")]
    public Text titleText;

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OnQuitClicked()
    {
        Debug.Log("Quit clicked.");
        Application.Quit();
    }
}
