using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Main menu controller with Start and Quit buttons.
/// Attach to a Canvas in the MainMenu scene.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public Button quitButton;
    public Text titleText;

    void Start()
    {
        Time.timeScale = 1f;

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
    }

    public void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
