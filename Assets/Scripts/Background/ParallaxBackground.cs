using UnityEngine;

/// <summary>
/// Infinite scrolling parallax background.
/// Attach to each background layer with a SpriteRenderer.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 1f;
    public bool autoScroll = true;

    [Header("Tiling")]
    public float spriteHeight = 10f;

    private Vector3 startPos;
    private float totalHeight;

    void Start()
    {
        startPos = transform.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteHeight = sr.bounds.size.y;
        }
        totalHeight = spriteHeight;
    }

    void Update()
    {
        if (autoScroll)
        {
            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

            // When the sprite has scrolled one full height, reset position
            if (transform.position.y <= startPos.y - totalHeight)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    startPos.y,
                    transform.position.z
                );
            }
        }
    }
}
