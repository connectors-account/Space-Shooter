using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game HUD display showing health, score, wave number, and active power-ups.
/// </summary>
public class HUDManager : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText;
    public Text waveText;
    public Text healthText;
    public Text powerUpText;
    public GameObject hudPanel;

    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnWaveChanged += UpdateWave;
            GameManager.Instance.OnGameStateChanged += OnStateChanged;
        }

        FindPlayerReferences();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnWaveChanged -= UpdateWave;
            GameManager.Instance.OnGameStateChanged -= OnStateChanged;
        }

        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateHealth;
    }

    public void FindPlayerReferences()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerShooting = player.GetComponent<PlayerShooting>();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealth;
                UpdateHealth(playerHealth.CurrentHealth, playerHealth.maxHealth);
            }
        }
    }

    private void Update()
    {
        UpdatePowerUpDisplay();
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score}";
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"WAVE {wave}";
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthText != null)
        {
            string hearts = "";
            for (int i = 0; i < max; i++)
            {
                hearts += i < current ? "\u2665 " : "\u2661 "; // filled / empty heart
            }
            healthText.text = hearts.Trim();
        }
    }

    private void UpdatePowerUpDisplay()
    {
        if (powerUpText == null || playerShooting == null || playerHealth == null) return;

        string text = "";
        if (playerShooting.HasRapidFire)
            text += "[RAPID FIRE] ";
        if (playerShooting.HasSpreadShot)
            text += "[SPREAD SHOT] ";
        if (playerHealth.HasShield)
            text += "[SHIELD] ";

        powerUpText.text = text;
    }

    private void OnStateChanged(GameState state)
    {
        if (hudPanel != null)
            hudPanel.SetActive(state == GameState.Playing);
    }
}
