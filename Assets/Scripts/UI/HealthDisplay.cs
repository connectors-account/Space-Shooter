using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// HealthDisplay manages the visual representation of player health.
/// Supports both icon-based (hearts) and bar-based display.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    [Header("Display Mode")]
    [SerializeField] private HealthDisplayMode displayMode = HealthDisplayMode.Icons;

    [Header("Icon Mode Settings")]
    [SerializeField] private Transform healthIconContainer;
    [SerializeField] private GameObject healthIconPrefab;
    [SerializeField] private Sprite fullHeartSprite;
    [SerializeField] private Sprite emptyHeartSprite;

    [Header("Bar Mode Settings")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Gradient healthGradient;

    // Private variables
    private List<Image> healthIcons = new List<Image>();
    private int maxHealth;

    public enum HealthDisplayMode
    {
        Icons,  // Heart icons
        Bar     // Health bar
    }

    /// <summary>
    /// Initialize health display with max health
    /// </summary>
    public void Initialize(int maxHealthValue)
    {
        maxHealth = maxHealthValue;

        if (displayMode == HealthDisplayMode.Icons)
        {
            CreateHealthIcons();
        }
    }

    /// <summary>
    /// Create health icon objects
    /// </summary>
    private void CreateHealthIcons()
    {
        // Clear existing icons
        foreach (var icon in healthIcons)
        {
            if (icon != null)
            {
                Destroy(icon.gameObject);
            }
        }
        healthIcons.Clear();

        // Create new icons
        if (healthIconContainer != null && healthIconPrefab != null)
        {
            for (int i = 0; i < maxHealth; i++)
            {
                GameObject iconObj = Instantiate(healthIconPrefab, healthIconContainer);
                Image iconImage = iconObj.GetComponent<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = fullHeartSprite;
                    healthIcons.Add(iconImage);
                }
            }
        }
    }

    /// <summary>
    /// Update the health display
    /// </summary>
    public void UpdateHealth(int currentHealth, int maxHealthValue)
    {
        // Update max health if changed
        if (maxHealthValue != maxHealth)
        {
            maxHealth = maxHealthValue;
            if (displayMode == HealthDisplayMode.Icons)
            {
                CreateHealthIcons();
            }
        }

        if (displayMode == HealthDisplayMode.Icons)
        {
            UpdateIconDisplay(currentHealth);
        }
        else
        {
            UpdateBarDisplay(currentHealth);
        }
    }

    /// <summary>
    /// Update icon-based health display
    /// </summary>
    private void UpdateIconDisplay(int currentHealth)
    {
        for (int i = 0; i < healthIcons.Count; i++)
        {
            if (healthIcons[i] != null)
            {
                if (i < currentHealth)
                {
                    healthIcons[i].sprite = fullHeartSprite;
                    healthIcons[i].color = Color.white;
                }
                else
                {
                    healthIcons[i].sprite = emptyHeartSprite;
                    healthIcons[i].color = new Color(1f, 1f, 1f, 0.5f);
                }
            }
        }
    }

    /// <summary>
    /// Update bar-based health display
    /// </summary>
    private void UpdateBarDisplay(int currentHealth)
    {
        if (healthBarFill != null)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = fillAmount;

            // Update color based on health percentage
            if (healthGradient != null)
            {
                healthBarFill.color = healthGradient.Evaluate(fillAmount);
            }
        }
    }

    /// <summary>
    /// Animate health loss (flash effect)
    /// </summary>
    public void AnimateHealthLoss()
    {
        StartCoroutine(FlashHealthDisplay());
    }

    /// <summary>
    /// Flash the health display red
    /// </summary>
    private System.Collections.IEnumerator FlashHealthDisplay()
    {
        if (displayMode == HealthDisplayMode.Bar && healthBarFill != null)
        {
            Color originalColor = healthBarFill.color;
            healthBarFill.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            healthBarFill.color = originalColor;
        }
    }
}
