using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScoreUI - Displays current score and wave number.
/// Attach to a UI Text element in the GamePlay scene Canvas.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("UI References")]
    public Text scoreText;
    public Text waveText;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            GameManager.Instance.OnWaveChanged += UpdateWaveDisplay;
            // Initialize
            UpdateScoreDisplay(GameManager.Instance.Score);
            UpdateWaveDisplay(GameManager.Instance.CurrentWave);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            GameManager.Instance.OnWaveChanged -= UpdateWaveDisplay;
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            UpdateScoreDisplay(GameManager.Instance.Score);
            UpdateWaveDisplay(GameManager.Instance.CurrentWave);
        }
    }

    public void UpdateScoreDisplay(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString("N0");
    }

    public void UpdateWaveDisplay(int wave)
    {
        if (waveText != null)
            waveText.text = "Wave " + wave;
    }
}
