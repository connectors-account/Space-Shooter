// ============================================================================
// BackgroundScroller.cs - Parallax scrolling star background
// ============================================================================
using UnityEngine;

/// <summary>
/// Creates a continuously scrolling background effect.
/// Attach to a quad/sprite that tiles vertically.
/// For parallax, use multiple instances at different speeds/layers.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    // ---- Configuration ----
    [Header("Scroll Settings")]
    [Tooltip("Scroll speed in units per second (positive = scrolls down)")]
    public float scrollSpeed = 2f;

    [Tooltip("Height of the background tile (used for seamless wrapping)")]
    public float tileHeight = 20f;

    // ---- Internal ----
    private Vector3 _startPosition;
    private Material _material;     // For material-based offset scrolling (optional)
    private bool _useMaterialScroll = false;

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================
    private void Start()
    {
        _startPosition = transform.position;

        // Check if we should use material UV offset scrolling
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.material != null && rend.material.HasProperty("_MainTex"))
        {
            _material = rend.material;
            _useMaterialScroll = true;
        }
    }

    private void Update()
    {
        if (_useMaterialScroll)
        {
            // UV offset scrolling (best for textured quads)
            Vector2 offset = _material.mainTextureOffset;
            offset.y += scrollSpeed * Time.deltaTime * 0.1f;
            _material.mainTextureOffset = offset;
        }
        else
        {
            // Transform-based scrolling with wrap-around
            transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime, Space.World);

            // When the background scrolls past its height, reset position
            if (transform.position.y <= _startPosition.y - tileHeight)
            {
                transform.position = _startPosition;
            }
        }
    }
}
