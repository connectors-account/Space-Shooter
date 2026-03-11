using UnityEngine;

/// <summary>
/// Scrolls a background quad downward to create the illusion of flying through space.
/// Attach this to a Quad with a tiling material.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Speed at which the background scrolls downward.")]
    public float scrollSpeed = 0.5f;

    // ---- Internal ----
    private Renderer rend;
    private Vector2 offset;

    // =========================================================================
    // Unity Lifecycle
    // =========================================================================

    private void Start()
    {
        rend = GetComponent<Renderer>();
        offset = Vector2.zero;
    }

    private void Update()
    {
        // Scroll the texture offset over time
        offset.y += scrollSpeed * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }
}
