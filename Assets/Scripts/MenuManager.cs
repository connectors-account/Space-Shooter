using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene-level menu transitions.
/// Can be used as a standalone menu scene or alongside UIManager.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // If GameManager exists in the same scene, use it
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            // Load the game scene directly
            SceneManager.LoadScene("GameScene");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
