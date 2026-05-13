// ============================================================================
// ParallaxBackground.cs - Multi-layer parallax scrolling background
// Creates a continuous vertically-scrolling starfield with parallax depth.
// ============================================================================
using UnityEngine;

/// <summary>
/// Scrolls a background sprite vertically and tiles it seamlessly.
/// Attach one instance per parallax layer. Deeper layers should have
/// a smaller scrollSpeed for the parallax effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Vertical scroll speed in units per second. Negative = upward on screen (stars move down).")]
    [SerializeField] private float scrollSpeed = -2f;
    [Tooltip("Optional slight horizontal drift for variety.")]
    [SerializeField] private float horizontalDrift = 0f;

    [Header("Tiling")]
    [Tooltip("Height of the sprite in world units. Used for seamless wrap-around.")]
    [SerializeField] private float spriteHeight = 20f;
    [Tooltip("If true, automatically calculates spriteHeight from the SpriteRenderer bounds.")]
    [SerializeField] private bool autoCalculateHeight = true;

    private Vector3 startPosition;
    private SpriteRenderer sr;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Start()
    {
        startPosition = transform.position;
        sr = GetComponent<SpriteRenderer>();

        if (autoCalculateHeight && sr != null && sr.sprite != null)
        {
            spriteHeight = sr.bounds.size.y;
        }
    }

    private void Update()
    {
        // Scroll.
        float offsetY = Mathf.Repeat(Time.time * scrollSpeed, spriteHeight);
        float offsetX = Mathf.Repeat(Time.time * horizontalDrift, 100f);
        transform.position = startPosition + new Vector3(horizontalDrift != 0f ? Mathf.Sin(offsetX) * 0.5f : 0f, offsetY, 0f);
    }
}
