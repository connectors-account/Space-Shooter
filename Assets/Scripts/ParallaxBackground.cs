// =============================================================================
// ParallaxBackground.cs
// Creates an infinitely scrolling background effect. Uses two copies of the
// background sprite that loop seamlessly. Supports multiple layers with
// different scroll speeds for a parallax depth effect.
// Attach this to each background layer GameObject.
// =============================================================================
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Scroll Settings
    // -------------------------------------------------------------------------
    [Header("Scroll Settings")]
    [Tooltip("Vertical scroll speed in units per second. Positive = scroll downward.")]
    public float scrollSpeed = 1f;

    [Tooltip("If true, scrolling is completely automatic (no camera dependency). " +
             "If false, movement is relative to the camera.")]
    public bool autoScroll = true;

    // -------------------------------------------------------------------------
    // Internal
    // -------------------------------------------------------------------------
    private float spriteHeight;
    private Vector3 startPosition;
    private Transform[] copies; // Two copies for seamless looping

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    /// <summary>
    /// Calculate sprite dimensions and create a second copy for looping.
    /// </summary>
    void Start()
    {
        startPosition = transform.position;

        // Get the height of the sprite for looping calculations
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            spriteHeight = sr.sprite.bounds.size.y * transform.localScale.y;
        }
        else
        {
            // Default height if no sprite is attached
            spriteHeight = 10f;
            Debug.LogWarning("ParallaxBackground: No SpriteRenderer found on " +
                             gameObject.name + ". Using default height of 10.");
        }

        // Create a second copy of this background positioned directly above
        CreateLoopCopy();
    }

    /// <summary>
    /// Scroll the background every frame and handle looping.
    /// </summary>
    void Update()
    {
        if (autoScroll)
        {
            // Move both this and the copy downward
            transform.position += Vector3.down * scrollSpeed * Time.deltaTime;
        }

        // Check if this sprite has scrolled below the screen enough to loop
        // When the sprite moves down by its full height, reset it above the copy
        if (transform.position.y <= startPosition.y - spriteHeight)
        {
            RepositionAbove();
        }
    }

    // -------------------------------------------------------------------------
    // Loop Setup
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a duplicate of this background sprite positioned directly above.
    /// Both sprites scroll together to create a seamless loop.
    /// </summary>
    private void CreateLoopCopy()
    {
        // Create a copy as a sibling object
        GameObject copy = new GameObject(gameObject.name + "_Copy");
        copy.transform.SetParent(transform.parent);
        copy.transform.position = new Vector3(
            transform.position.x,
            transform.position.y + spriteHeight,
            transform.position.z
        );
        copy.transform.localScale = transform.localScale;

        // Copy the SpriteRenderer component
        SpriteRenderer originalSR = GetComponent<SpriteRenderer>();
        if (originalSR != null)
        {
            SpriteRenderer copySR = copy.AddComponent<SpriteRenderer>();
            copySR.sprite = originalSR.sprite;
            copySR.color = originalSR.color;
            copySR.sortingLayerName = originalSR.sortingLayerName;
            copySR.sortingOrder = originalSR.sortingOrder;
            copySR.material = originalSR.material;
        }

        // Attach a ParallaxBackground component to the copy with the same settings
        ParallaxBackground copyBG = copy.AddComponent<ParallaxBackground>();
        copyBG.scrollSpeed = this.scrollSpeed;
        copyBG.autoScroll = this.autoScroll;
    }

    /// <summary>
    /// Repositions this sprite above the visible area to create endless looping.
    /// </summary>
    private void RepositionAbove()
    {
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + spriteHeight * 2f,
            transform.position.z
        );
    }
}
