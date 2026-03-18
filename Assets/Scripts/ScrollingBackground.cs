using UnityEngine;

/// <summary>
/// Scrolls a background quad/sprite downward to create a parallax space effect.
/// Attach this to a background quad with a tiling material.
/// If you use a simple sprite, this will move the object and loop it.
/// </summary>
public class ScrollingBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 0.5f;
    public bool useRendererOffset = true; // true = offset material UV, false = move transform

    private Renderer rend;
    private float backgroundHeight;
    private Vector3 startPos;

    void Start()
    {
        rend = GetComponent<Renderer>();
        startPos = transform.position;

        if (!useRendererOffset)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                backgroundHeight = sr.bounds.size.y;
            }
        }
    }

    void Update()
    {
        if (useRendererOffset && rend != null)
        {
            // Scroll the material's texture offset (best for tiled materials)
            float offset = Time.time * scrollSpeed;
            rend.material.mainTextureOffset = new Vector2(0f, offset);
        }
        else
        {
            // Move the transform downward and loop
            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

            if (transform.position.y <= startPos.y - backgroundHeight)
            {
                transform.position = startPos;
            }
        }
    }
}
