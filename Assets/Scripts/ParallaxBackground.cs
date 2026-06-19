using UnityEngine;

/// <summary>
/// Simple vertical parallax scroller. Attach to one or more background layers
/// (each made of two stacked sprites). The layer scrolls downward and each
/// sprite that leaves the bottom is recycled to the top, creating an endless
/// loop. Use different speeds on different layers for a parallax effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("Scroll speed for this layer (world units per second). " +
             "Use smaller values for far layers, larger for near layers.")]
    public float scrollSpeed = 1.5f;

    [Tooltip("The two (or more) tile sprites that make up this layer.")]
    public Transform[] tiles;

    [Tooltip("Vertical height of a single tile in world units. " +
             "If 0 it is auto-detected from the first tile's SpriteRenderer.")]
    public float tileHeight = 0f;

    private float totalHeight;

    private void Start()
    {
        if (tiles == null || tiles.Length < 2)
        {
            Debug.LogWarning("ParallaxBackground needs at least 2 tiles to loop.");
            return;
        }

        // Auto-detect tile height from the sprite bounds if not provided.
        if (tileHeight <= 0f)
        {
            SpriteRenderer sr = tiles[0].GetComponent<SpriteRenderer>();
            tileHeight = sr != null ? sr.bounds.size.y : 10f;
        }

        totalHeight = tileHeight * tiles.Length;
    }

    private void Update()
    {
        if (tiles == null || tiles.Length < 2) return;

        foreach (Transform tile in tiles)
        {
            // Move the tile downward.
            tile.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // When a tile drops a full layer-height below its loop point,
            // wrap it back to the top to create a seamless scroll.
            if (tile.position.y <= -totalHeight + tileHeight)
                tile.position += Vector3.up * totalHeight;
        }
    }
}
