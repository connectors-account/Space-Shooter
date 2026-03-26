using UnityEngine;

/// <summary>
/// Scrolls multiple background layers at different speeds to create
/// a parallax depth effect. Each layer auto-loops when it scrolls off screen.
/// Attach to a parent GameObject whose children are the individual layers.
/// Each child should have a SpriteRenderer with a vertically-tiling sprite.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        [Tooltip("The Transform holding the SpriteRenderer for this layer.")]
        public Transform layerTransform;
        [Tooltip("Scroll speed in world-units per second (higher = closer / faster).")]
        public float scrollSpeed = 1f;
    }

    [Header("Layers (back-to-front order)")]
    public Layer[] layers = new Layer[2];

    private float[] spriteHeights;

    private void Start()
    {
        spriteHeights = new float[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerTransform == null) continue;
            SpriteRenderer sr = layers[i].layerTransform.GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                spriteHeights[i] = sr.bounds.size.y;
            else
                spriteHeights[i] = 20f; // fallback
        }
    }

    private void Update()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerTransform == null) continue;

            Transform t = layers[i].layerTransform;
            t.Translate(Vector3.down * layers[i].scrollSpeed * Time.deltaTime, Space.World);

            // When the sprite has scrolled one full height downward, snap it back
            if (t.position.y <= -spriteHeights[i])
            {
                Vector3 pos = t.position;
                pos.y += spriteHeights[i] * 2f; // two sprites tiled
                t.position = pos;
            }
        }
    }
}
