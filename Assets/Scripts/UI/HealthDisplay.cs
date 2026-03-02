using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HealthDisplay connects the player health to the UI.
/// Automatically updates when player health changes.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to player health component")]
    public PlayerHealth playerHealth;
    
    [Tooltip("Health bar slider")]
    public Slider healthSlider;
    
    [Tooltip("Health text display")]
    public Text healthText;
    
    [Tooltip("Health bar fill image for color changes")]
    public Image healthFill;

    [Header("Color Settings")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    
    [Tooltip("Health percentage threshold for low health color")]
    [Range(0, 1)]
    public float lowHealthThreshold = 0.3f;

    void Start()
    {
        // Find player health if not assigned
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }
        
        // Subscribe to health changes
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthDisplay);
            
            // Initialize display
            UpdateHealthDisplay(playerHealth.GetCurrentHealth(), playerHealth.GetMaxHealth());
        }
    }

    /// <summary>
    /// Update the health display
    /// </summary>
    void UpdateHealthDisplay(int currentHealth, int maxHealth)
    {
        // Update slider
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        // Update text
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString() + " / " + maxHealth.ToString();
        }
        
        // Update color based on health percentage
        if (healthFill != null)
        {
            float healthPercent = (float)currentHealth / maxHealth;
            healthFill.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        }
        
        // Also update UIManager if available
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthDisplay);
        }
    }
}
