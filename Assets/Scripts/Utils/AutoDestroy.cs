using UnityEngine;

/// <summary>
/// AutoDestroy automatically destroys or deactivates a GameObject after a duration.
/// Useful for effects, particles, and temporary objects.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private bool usePooling = true; // Deactivate instead of destroy
    [SerializeField] private bool startOnEnable = true;

    private float timer;

    private void OnEnable()
    {
        if (startOnEnable)
        {
            ResetTimer();
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (usePooling)
            {
                gameObject.SetActive(false);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Reset the timer to full lifetime
    /// </summary>
    public void ResetTimer()
    {
        timer = lifetime;
    }

    /// <summary>
    /// Set a custom lifetime
    /// </summary>
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
        timer = lifetime;
    }
}
