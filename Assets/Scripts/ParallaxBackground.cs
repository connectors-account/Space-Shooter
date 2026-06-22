using UnityEngine;

/// <summary>
/// Creates a seamless, infinitely scrolling vertical background to give the
/// sense of flying through space. Works with two identical background sprites
/// stacked vertically: when one scrolls fully off the bottom, it leaps back
/// above the other, creating an endless loop.
///
/// Attach this to a parent object that holds two child sprite renderers, or
/// assign the two layers manually. Multiple instances at different speeds
/// produce a layered parallax effect (e.g. slow far stars + fast near stars).
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [Tooltip("Vertical scroll speed in units/second.")]
    public float scrollSpeed = 1f;

    [Tooltip("The two background tiles stacked vertically. Each should be one screen tall.")]
    public Transform[] layers;

    [Tooltip("Height of a single background tile in world units. Set to your sprite's height.")]
    public float tileHeight = 10f;

    private void Update()
    {
        if (layers == null || layers.Length == 0)
            return;

        foreach (Transform layer in layers)
        {
            if (layer == null)
                continue;

            // Scroll the tile downward.
            layer.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // Once a tile has moved a full tile-height below the start, wrap it
            // back up above the others to keep the scroll seamless.
            if (layer.position.y <= -tileHeight)
            {
                // Reposition to the top: total span = tileHeight * number of tiles.
                float wrapOffset = tileHeight * layers.Length;
                layer.position += Vector3.up * wrapOffset;
            }
        }
    }
}
