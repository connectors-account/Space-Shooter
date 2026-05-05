using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles Main Menu button interactions.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "GameScene";

    public void OnStartGameButtonPressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnQuitButtonPressed()
    {
        Application.Quit();
    }
}
