using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Game Over screen buttons and display.
/// Attach to the GameOverPanel.
/// </summary>
public class GameOverController : MonoBehaviour
{
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text gameOverTitle;

    private float animTimer = 0f;

    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestart);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuit);
    }

    private void OnEnable()
    {
        animTimer = 0f;
    }

    private void Update()
    {
        // Simple color pulse on "GAME OVER" text
        if (gameOverTitle != null)
        {
            animTimer += Time.unscaledDeltaTime;
            float t = 0.5f + 0.5f * Mathf.Sin(animTimer * 3f);
            gameOverTitle.color = Color.Lerp(Color.red, Color.yellow, t);
        }
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
