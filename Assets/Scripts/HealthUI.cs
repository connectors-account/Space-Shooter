using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HealthUI - Displays player lives using heart icons or a text counter.
/// Attach to a UI Text or Image container in the GamePlay scene Canvas.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Text livesText;
    public Image[] heartImages;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLivesChanged += UpdateLivesDisplay;
            // Initialize display
            UpdateLivesDisplay(GameManager.Instance.PlayerLives);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLivesChanged -= UpdateLivesDisplay;
    }

    private void Start()
    {
        // Fallback init if GameManager was already running
        if (GameManager.Instance != null)
            UpdateLivesDisplay(GameManager.Instance.PlayerLives);
    }

    /// <summary>
    /// Update the lives display: either text counter or heart icon visibility.
    /// </summary>
    public void UpdateLivesDisplay(int lives)
    {
        // Text-based display
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives;
        }

        // Icon-based display
        if (heartImages != null)
        {
            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                    heartImages[i].enabled = i < lives;
            }
        }
    }
}
