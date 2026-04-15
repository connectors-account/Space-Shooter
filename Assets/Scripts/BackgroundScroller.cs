using UnityEngine;

/// <summary>
/// Simple parallax-style background scroll by changing texture offset.
/// Attach to a Quad/Sprite with a material that tiles vertically.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.2f;

    private Renderer cachedRenderer;
    private Vector2 currentOffset;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        currentOffset.y += scrollSpeed * Time.deltaTime;

        if (cachedRenderer != null && cachedRenderer.material != null)
        {
            cachedRenderer.material.mainTextureOffset = currentOffset;
        }
    }
}
