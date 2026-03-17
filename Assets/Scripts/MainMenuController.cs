using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Simple main-menu controller.  Attach to a Canvas in the MainMenu scene.
/// Wire Play / Quit buttons in the Inspector.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Text titleText;

    private void Start()
    {
        Time.timeScale = 1f;
        if (titleText != null)
            titleText.text = "SPACE SHOOTER";
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
