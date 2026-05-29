using UnityEngine;

/// <summary>
/// Parallax scrolling background. Attach to a quad/sprite with a tiling material.
/// Create two background layers at different scroll speeds for depth.
/// 
/// Setup:
/// 1. Create a Quad and assign a star-field material
/// 2. Set the material's Wrap Mode to Repeat
/// 3. Attach this script and set scrollSpeed
/// 4. Duplicate for a second parallax layer at a different speed
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 0.5f;
    public Vector2 scrollDirection = Vector2.down;

    private Renderer bgRenderer;
    private Vector2 offset;

    void Start()
    {
        bgRenderer = GetComponent<Renderer>();
        if (bgRenderer == null)
        {
            Debug.LogWarning("BackgroundScroller: No Renderer found on " + gameObject.name);
        }
    }

    void Update()
    {
        if (bgRenderer == null) return;

        offset += scrollDirection.normalized * scrollSpeed * Time.deltaTime;
        bgRenderer.material.mainTextureOffset = offset;
    }
}
