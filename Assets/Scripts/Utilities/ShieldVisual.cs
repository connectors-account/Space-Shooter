// ============================================================================
// ShieldVisual.cs - Visual indicator for the player's energy shield
// Shows/hides and pulses based on the shield's current state.
// ============================================================================
using UnityEngine;

/// <summary>
/// Attach as a child of the Player. Shows a translucent shield bubble
/// when the player has shield points, and hides when shield is depleted.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class ShieldVisual : MonoBehaviour
{
    private SpriteRenderer sr;
    private PlayerHealth playerHealth;
    private float baseAlpha = 0.5f;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Find the PlayerHealth on the parent.
        playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged += UpdateVisual;
        }

        // Start hidden.
        sr.enabled = false;
    }

    private void Update()
    {
        if (sr.enabled)
        {
            // Gentle pulse effect.
            float pulse = 0.3f + Mathf.PingPong(Time.time * 0.5f, 0.3f);
            Color c = sr.color;
            c.a = pulse;
            sr.color = c;

            // Slow rotation.
            transform.Rotate(0f, 0f, 20f * Time.deltaTime);
        }
    }

    private void UpdateVisual(int current, int max)
    {
        sr.enabled = current > 0;
        if (current > 0 && max > 0)
        {
            // Scale alpha with shield percentage.
            baseAlpha = 0.2f + 0.5f * ((float)current / max);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnShieldChanged -= UpdateVisual;
        }
    }
}
