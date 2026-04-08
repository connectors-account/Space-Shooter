using UnityEngine;

/// <summary>
/// Creates a parallax scrolling starfield background effect.
/// Can be used for multiple background layers at different speeds.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;
    public float resetPositionY = -20f;
    public float startPositionY = 20f;

    [Header("Parallax Layer")]
    [Tooltip("Lower values = further away / slower (0.1 - 1.0)")]
    public float parallaxFactor = 1f;

    private float spriteHeight;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Try to get sprite height for proper tiling
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteHeight = sr.bounds.size.y;
        }
        else
        {
            spriteHeight = Mathf.Abs(startPositionY - resetPositionY);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isPaused)
            return;

        float effectiveSpeed = scrollSpeed * parallaxFactor;
        transform.position += Vector3.down * effectiveSpeed * Time.deltaTime;

        // Reset position for seamless scrolling
        if (transform.position.y <= resetPositionY)
        {
            transform.position = new Vector3(transform.position.x, startPositionY, transform.position.z);
        }
    }
}
