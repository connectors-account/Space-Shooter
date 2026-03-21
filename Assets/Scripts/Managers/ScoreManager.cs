using UnityEngine;

/// <summary>
/// Tracks and manages the player's score with combo multiplier support.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Combo Settings")]
    public float comboWindowSeconds = 2f;
    public int maxComboMultiplier = 5;

    private int comboCount;
    private float lastKillTime;

    public int ComboMultiplier => Mathf.Min(comboCount, maxComboMultiplier);

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Award points for destroying an enemy, applying combo multiplier.
    /// </summary>
    public void AwardKillPoints(int basePoints)
    {
        float timeSinceLastKill = Time.time - lastKillTime;

        if (timeSinceLastKill <= comboWindowSeconds)
        {
            comboCount++;
        }
        else
        {
            comboCount = 1;
        }

        lastKillTime = Time.time;
        int multiplier = Mathf.Min(comboCount, maxComboMultiplier);
        int totalPoints = basePoints * multiplier;

        GameManager.Instance?.AddScore(totalPoints);

        if (multiplier > 1)
        {
            UIManager.Instance?.ShowCombo(multiplier);
        }
    }

    /// <summary>Reset the combo counter.</summary>
    public void ResetCombo()
    {
        comboCount = 0;
    }
}
