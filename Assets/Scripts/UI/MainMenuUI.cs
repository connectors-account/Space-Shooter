using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu screen with Start and Quit buttons.
/// Also displays high score.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public Text titleText;
    public Text highScoreText;
    public Button startButton;
    public Button quitButton;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnStateChanged;
            OnStateChanged(GameManager.Instance.CurrentState);
        }

        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        UpdateHighScore();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    private void OnStartClicked()
    {
        // Find and reset the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(true);
            player.transform.position = new Vector3(0, -3.5f, 0);

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.ResetHealth();

            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            if (shooting != null) shooting.ResetShooting();
        }

        // Re-link HUD to player
        HUDManager hud = FindObjectOfType<HUDManager>();
        if (hud != null) hud.FindPlayerReferences();

        GameManager.Instance?.StartGame();
    }

    private void OnQuitClicked()
    {
        GameManager.Instance?.QuitGame();
    }

    private void UpdateHighScore()
    {
        if (highScoreText != null && GameManager.Instance != null)
        {
            int hs = GameManager.Instance.HighScore;
            highScoreText.text = hs > 0 ? $"HIGH SCORE: {hs}" : "";
        }
    }

    private void OnStateChanged(GameState state)
    {
        bool showMenu = state == GameState.MainMenu;
        if (menuPanel != null)
            menuPanel.SetActive(showMenu);

        if (showMenu)
            UpdateHighScore();
    }
}
