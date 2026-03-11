using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    public Image fillImage;
    public HealthSystem targetHealth;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f;
    public float midHealthThreshold = 0.6f;

    [Header("Animation")]
    public float smoothSpeed = 5f;
    public bool animateChanges = true;

    private float targetValue;

    private void Start()
    {
        if (targetHealth == null)
        {
            // Try to find player's health
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                targetHealth = player.GetComponent<HealthSystem>();
            }
        }

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += OnHealthChanged;
            OnHealthChanged(targetHealth.CurrentHealth, targetHealth.maxHealth);
        }
    }

    private void Update()
    {
        if (animateChanges && healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * smoothSpeed);
            UpdateColor(healthSlider.value / healthSlider.maxValue);
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthSlider == null) return;

        healthSlider.maxValue = max;
        targetValue = current;

        if (!animateChanges)
        {
            healthSlider.value = current;
            UpdateColor((float)current / max);
        }
    }

    private void UpdateColor(float healthPercent)
    {
        if (fillImage == null) return;

        if (healthPercent <= lowHealthThreshold)
        {
            fillImage.color = lowHealthColor;
        }
        else if (healthPercent <= midHealthThreshold)
        {
            fillImage.color = midHealthColor;
        }
        else
        {
            fillImage.color = fullHealthColor;
        }
    }

    private void OnDestroy()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= OnHealthChanged;
        }
    }
}
