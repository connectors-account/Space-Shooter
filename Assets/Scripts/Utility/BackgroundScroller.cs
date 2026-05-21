using UnityEngine;

/// <summary>
/// BackgroundScroller - Scrolls a tiling background material for a parallax star-field effect.
/// Attach to a Quad that uses an Unlit/Texture material with a tiled star texture.
/// Or simply use this with a solid-color quad; the scroll still works if you add a texture later.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 0.1f;
    public Vector2 scrollDirection = new Vector2(0f, -1f);

    private Renderer bgRenderer;
    private Vector2 offset;

    private void Start()
    {
        bgRenderer = GetComponent<Renderer>();
        offset = Vector2.zero;
    }

    private void Update()
    {
        if (bgRenderer == null) return;

        offset += scrollDirection * scrollSpeed * Time.deltaTime;
        bgRenderer.material.mainTextureOffset = offset;
    }
}
