using UnityEngine;

/// <summary>
/// Scrolls a background sprite downward in a loop, creating a parallax effect.
/// Attach to each background layer. Uses two copies that swap.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    public float scrollSpeed = 1f;
    public bool autoSetup = true;

    private float spriteHeight;
    private Vector3 startPos;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            spriteHeight = sr.bounds.size.y;
        }
        else
        {
            spriteHeight = 12f; // default
        }
        startPos = transform.position;
    }

    void Update()
    {
        // Scroll down
        float newY = Mathf.Repeat(Time.time * scrollSpeed, spriteHeight);
        transform.position = startPos + Vector3.down * newY;
    }
}
