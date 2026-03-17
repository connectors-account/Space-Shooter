using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game HUD displaying score, health, and wave number.
/// Attach to a Canvas GameObject named "HUDCanvas".
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    [Header("UI References")]
    public Text scoreText;
    public Text healthText;
    public Text waveText;
    public Slider healthBar;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"SCORE: {score}";
    }

    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"HP: {current}/{max}";

        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
        }
    }

    public void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"WAVE {wave}";
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
