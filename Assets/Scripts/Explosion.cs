// =============================================================================
// Explosion.cs
// Simple explosion effect that plays an animation (or just scales up and
// fades out) and then self-destructs. Attach to the explosion prefab.
// Works with or without an Animator component.
// =============================================================================
using UnityEngine;

public class Explosion : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Settings
    // -------------------------------------------------------------------------
    [Header("Explosion Settings")]
    [Tooltip("How long the explosion effect lasts before self-destructing.")]
    public float duration = 0.5f;

    [Tooltip("Maximum scale the explosion grows to.")]
    public float maxScale = 2f;

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private float timer = 0f;
    private Vector3 initialScale;
    private SpriteRenderer spriteRenderer;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Cache references and set initial state.
    /// </summary>
    void Start()
    {
        initialScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        timer = 0f;
    }

    /// <summary>
    /// Animate the explosion: scale up and fade out, then self-destruct.
    /// </summary>
    void Update()
    {
        timer += Time.deltaTime;
        float progress = timer / duration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Scale up
        float scaleMultiplier = Mathf.Lerp(1f, maxScale, progress);
        transform.localScale = initialScale * scaleMultiplier;

        // Fade out
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f - progress;
            spriteRenderer.color = c;
        }
    }
}
