using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "GamePlay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void StartGameFromMainMenu()
    {
        AudioManager.Instance?.PlaySfx(AudioSfx.ButtonClick);
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void ReturnToMainMenu()
    {
        AudioManager.Instance?.PlaySfx(AudioSfx.ButtonClick);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void StartGameplayInCurrentScene()
    {
        AudioManager.Instance?.PlaySfx(AudioSfx.ButtonClick);
        GameManager.Instance?.StartGame();
    }

    public void TogglePause()
    {
        AudioManager.Instance?.PlaySfx(AudioSfx.ButtonClick);
        GameManager.Instance?.TogglePause();
    }

    public void RestartLevel()
    {
        AudioManager.Instance?.PlaySfx(AudioSfx.ButtonClick);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlaySfx(AudioSfx.ButtonClick);
        GameManager.Instance?.QuitGame();
    }
}
