using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game Over screen showing final score and options to retry or return to menu.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public Text gameOverText;
    public Text finalScoreText;
    public Text newHighScoreText;
    public Button retryButton;
    public Button mainMenuButton;

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnStateChanged;

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
    }

    private void OnRetryClicked()
    {
        CleanupGameObjects();

        // Reset and restart
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(true);
            player.transform.position = new Vector3(0, -3.5f, 0);
            player.transform.rotation = Quaternion.identity;

            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null) health.ResetHealth();

            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            if (shooting != null) shooting.ResetShooting();
        }

        HUDManager hud = FindObjectOfType<HUDManager>();
        if (hud != null) hud.FindPlayerReferences();

        GameManager.Instance?.StartGame();
    }

    private void OnMainMenuClicked()
    {
        CleanupGameObjects();
        GameManager.Instance?.ReturnToMainMenu();
    }

    private void CleanupGameObjects()
    {
        // Return all active enemies and bullets to pool
        EnemyBase[] enemies = FindObjectsOfType<EnemyBase>();
        foreach (var enemy in enemies)
        {
            if (enemy.gameObject.activeInHierarchy && ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(enemy.poolTag, enemy.gameObject);
        }

        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (var bullet in bullets)
        {
            if (bullet.gameObject.activeInHierarchy && ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(bullet.poolTag, bullet.gameObject);
        }

        PowerUp[] powerUps = FindObjectsOfType<PowerUp>();
        foreach (var pu in powerUps)
        {
            if (pu.gameObject.activeInHierarchy && ObjectPool.Instance != null)
                ObjectPool.Instance.ReturnToPool(pu.poolTag, pu.gameObject);
        }

        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null) spawner.StopSpawning();
    }

    private void OnStateChanged(GameState state)
    {
        bool show = state == GameState.GameOver;
        if (gameOverPanel != null)
            gameOverPanel.SetActive(show);

        if (show)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (GameManager.Instance == null) return;

        if (finalScoreText != null)
            finalScoreText.text = $"SCORE: {GameManager.Instance.Score}";

        if (newHighScoreText != null)
        {
            bool isNewHigh = GameManager.Instance.Score >= GameManager.Instance.HighScore
                             && GameManager.Instance.Score > 0;
            newHighScoreText.text = isNewHigh ? "NEW HIGH SCORE!" : "";
            newHighScoreText.gameObject.SetActive(isNewHigh);
        }
    }
}
