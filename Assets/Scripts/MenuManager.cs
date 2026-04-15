using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls main menu actions.
/// </summary>
public class MenuManager : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "GameScene";

    public void StartGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
