using UnityEngine;

/// <summary>
/// Creates a parallax scrolling background effect.
/// Attach to each background layer GameObject with a SpriteRenderer.
/// Multiple instances at different speeds create depth illusion.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 1f;
    [SerializeField] private bool autoScroll = true;

    [Header("Tiling")]
    [SerializeField] private float spriteHeight = 20f; // Height of the background sprite in world units

    // Internal
    private Vector3 startPosition;
    private float resetPositionY;

    private void Start()
    {
        startPosition = transform.position;
        resetPositionY = -spriteHeight;
    }

    private void Update()
    {
        if (!autoScroll) return;

        // Scroll downward
        transform.Translate(Vector3.down * scrollSpeed * Time.deltaTime);

        // Reset position when scrolled past one full sprite height for seamless loop
        if (transform.position.y <= startPosition.y + resetPositionY)
        {
            Vector3 pos = transform.position;
            pos.y += spriteHeight * 2f; // Jump ahead by 2 sprite heights (assumes 2 copies)
            transform.position = pos;
        }
    }

    /// <summary>
    /// Set the scroll speed dynamically (e.g., to intensify during wave transitions).
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
}
