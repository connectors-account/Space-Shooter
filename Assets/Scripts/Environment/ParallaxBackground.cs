using UnityEngine;

/// <summary>
/// Scrolling parallax background layer. Attach to each background layer.
/// Create multiple layers with different scroll speeds for depth effect.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 1f;
    public bool scrollHorizontal;
    public float tileHeight = 10f; // height of the sprite in world units

    private Vector3 startPosition;
    private float scrollLength;

    private void Start()
    {
        startPosition = transform.position;
        scrollLength = tileHeight;
    }

    private void Update()
    {
        // Vertical scrolling
        float offset = Time.time * scrollSpeed;
        float yPos = Mathf.Repeat(offset, scrollLength);
        transform.position = startPosition + Vector3.down * yPos;
    }
}
