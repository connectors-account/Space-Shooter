using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Additional pause menu logic. Normally the UIManager handles show/hide.
/// This script adds keyboard shortcuts and visual setup.
/// Attach to the PauseMenuPanel.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private void Start()
    {
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResume);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
    }

    public void OnResume()
    {
        GameManager.Instance?.ResumeGame();
    }

    public void OnRestart()
    {
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenu()
    {
        GameManager.Instance?.GoToMainMenu();
    }

    public void OnQuit()
    {
        GameManager.Instance?.QuitGame();
    }
}
