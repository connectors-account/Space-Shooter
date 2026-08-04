using UnityEngine;

/// <summary>
/// Scrolls a sprite downward and seamlessly loops it.
/// Use TWO identical sprites stacked vertically (tile B placed directly above tile A).
/// Attach this script to BOTH sprites. They will scroll in sync and wrap individually,
/// creating an infinite starfield without gaps.
///
/// Assign a large star-field sprite (e.g. 1024x2048 px, set Wrap Mode = Repeat).
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("Downward scroll speed in world units per second.")]
    public float scrollSpeed = 2f;

    float      tileHeight;
    Vector3    startPos;

    void Start()
    {
        startPos = transform.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        tileHeight = sr != null ? sr.bounds.size.y : 10f;
    }

    void Update()
    {
        transform.position += Vector3.down * scrollSpeed * Time.deltaTime;

        // When this tile has scrolled one full height below its start, snap back up
        if (transform.position.y <= startPos.y - tileHeight)
            transform.position = startPos;
    }
}
