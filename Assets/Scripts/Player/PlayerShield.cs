using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    [Header("Shield Settings")]
    public float shieldDuration = 5f;
    public Color shieldColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;

    private SpriteRenderer spriteRenderer;
    private bool isActive = false;
    private float deactivateTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // Pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = Vector3.one * pulse;

        // Check if shield should deactivate
        if (Time.time >= deactivateTime)
        {
            Deactivate();
        }
    }

    public void Activate(float duration)
    {
        gameObject.SetActive(true);
        isActive = true;
        deactivateTime = Time.time + duration;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = shieldColor;
        }
    }

    public void Deactivate()
    {
        isActive = false;
        gameObject.SetActive(false);
    }

    public bool IsActive()
    {
        return isActive;
    }
}
