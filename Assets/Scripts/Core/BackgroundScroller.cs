using UnityEngine;

/// <summary>
/// BackgroundScroller creates a parallax scrolling star-field effect.
/// It moves two copies of a background quad downward in a loop,
/// resetting each to the top when it scrolls off-screen.
///
/// For a simple setup, attach this to a parent GameObject and
/// assign two child quads (Background1 and Background2) as the
/// scrolling layers. Each quad should be the size of the camera view.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    // ============================================================
    // CONFIGURATION
    // ============================================================

    [Header("Scroll Settings")]
    [Tooltip("Scroll speed in units per second (downward)")]
    public float scrollSpeed = 1.5f;

    [Tooltip("Height of each background tile (should match camera height)")]
    public float tileHeight = 10f;

    [Header("Background Layers")]
    [Tooltip("First background quad")]
    public Transform background1;

    [Tooltip("Second background quad (placed directly above the first)")]
    public Transform background2;

    // ============================================================
    // OPTIONAL PARALLAX
    // ============================================================
    [Header("Parallax (Optional)")]
    [Tooltip("Additional slow-moving layer for depth effect")]
    public Transform farBackground1;
    public Transform farBackground2;

    [Tooltip("Parallax speed multiplier for the far layer (0-1)")]
    public float parallaxFactor = 0.5f;

    // ============================================================
    // UNITY LIFECYCLE
    // ============================================================

    void Update()
    {
        // Scroll the main background layer
        ScrollLayer(background1, background2, scrollSpeed);

        // Scroll the far parallax layer at a slower speed
        if (farBackground1 != null && farBackground2 != null)
        {
            ScrollLayer(farBackground1, farBackground2, scrollSpeed * parallaxFactor);
        }
    }

    // ============================================================
    // SCROLLING LOGIC
    // ============================================================

    /// <summary>
    /// Move two tiles downward. When a tile goes fully off-screen,
    /// teleport it above the other tile to create an infinite loop.
    /// </summary>
    void ScrollLayer(Transform tile1, Transform tile2, float speed)
    {
        if (tile1 == null || tile2 == null) return;

        // Move both tiles down
        float delta = speed * Time.deltaTime;
        tile1.position += Vector3.down * delta;
        tile2.position += Vector3.down * delta;

        // Check if tile1 scrolled fully off the bottom
        if (tile1.position.y <= -tileHeight)
        {
            // Place it above tile2
            tile1.position = new Vector3(
                tile1.position.x,
                tile2.position.y + tileHeight,
                tile1.position.z
            );
        }

        // Check if tile2 scrolled fully off the bottom
        if (tile2.position.y <= -tileHeight)
        {
            // Place it above tile1
            tile2.position = new Vector3(
                tile2.position.x,
                tile1.position.y + tileHeight,
                tile2.position.z
            );
        }
    }
}
