using UnityEngine;

/// <summary>
/// ParallaxBackground creates a scrolling parallax effect for space backgrounds.
/// Supports multiple layers with different scroll speeds.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private float parallaxMultiplier = 1f;
    [SerializeField] private bool autoScroll = true;

    [Header("Tile Settings")]
    [SerializeField] private bool infiniteScroll = true;
    [SerializeField] private float tileHeight = 10f;

    // Private variables
    private Vector3 startPosition;
    private float offset;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Calculate tile height from sprite if available
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            tileHeight = spriteRenderer.bounds.size.y;
        }
    }

    private void Update()
    {
        // Don't scroll if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        if (autoScroll)
        {
            Scroll();
        }
    }

    /// <summary>
    /// Scroll the background
    /// </summary>
    private void Scroll()
    {
        offset += scrollSpeed * parallaxMultiplier * Time.deltaTime;

        // Apply scroll
        transform.position = startPosition + Vector3.down * offset;

        // Reset position for infinite scroll
        if (infiniteScroll && offset >= tileHeight)
        {
            offset = 0f;
            transform.position = startPosition;
        }
    }

    /// <summary>
    /// Set scroll speed externally
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }

    /// <summary>
    /// Set parallax multiplier (for layered backgrounds)
    /// </summary>
    public void SetParallaxMultiplier(float multiplier)
    {
        parallaxMultiplier = multiplier;
    }
}

/// <summary>
/// ParallaxLayer manages multiple background layers with different speeds
/// </summary>
public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public string name;
        public SpriteRenderer spriteRenderer;
        [Range(0f, 2f)] public float speedMultiplier = 1f;
        public int sortingOrder = 0;
    }

    [Header("Parallax Layers")]
    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private float baseScrollSpeed = 2f;

    private void Start()
    {
        // Setup sorting orders
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer != null)
            {
                layer.spriteRenderer.sortingOrder = layer.sortingOrder;

                // Add ParallaxBackground component if not present
                ParallaxBackground parallax = layer.spriteRenderer.GetComponent<ParallaxBackground>();
                if (parallax == null)
                {
                    parallax = layer.spriteRenderer.gameObject.AddComponent<ParallaxBackground>();
                }

                parallax.SetScrollSpeed(baseScrollSpeed);
                parallax.SetParallaxMultiplier(layer.speedMultiplier);
            }
        }
    }

    /// <summary>
    /// Change scroll speed for all layers
    /// </summary>
    public void SetGlobalScrollSpeed(float speed)
    {
        baseScrollSpeed = speed;
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer != null)
            {
                ParallaxBackground parallax = layer.spriteRenderer.GetComponent<ParallaxBackground>();
                if (parallax != null)
                {
                    parallax.SetScrollSpeed(speed);
                }
            }
        }
    }
}
