using UnityEngine;

/// <summary>
/// Creates an infinite scrolling parallax background.
/// Attach to each background layer. Uses two sprites to tile vertically.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    public float scrollSpeed = 1f;
    public bool autoSetup = true;

    private float spriteHeight;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteHeight = spriteRenderer.bounds.size.y;
        }
        else
        {
            spriteHeight = 20f; // fallback
        }
        startPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        // Scroll downward continuously
        float newY = Mathf.Repeat(Time.time * scrollSpeed, spriteHeight);
        transform.position = startPosition + Vector3.down * newY;
    }
}
