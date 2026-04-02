using UnityEngine;

/// <summary>
/// Creates a scrolling parallax background effect.
/// Attach to background sprite objects. Multiple layers at different speeds create depth.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private bool scrollHorizontal = false;
    [SerializeField] private float parallaxFactor = 1f;

    [Header("Looping")]
    [SerializeField] private bool enableLooping = true;
    [SerializeField] private float resetPositionY = 20f;
    [SerializeField] private float startPositionY = -20f;

    private Vector3 startPos;
    private float spriteHeight;
    private float spriteWidth;
    private SpriteRenderer spriteRenderer;

    // For material-based scrolling (alternative approach)
    private Material mat;
    private bool useMaterialScroll;

    private void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            spriteHeight = spriteRenderer.bounds.size.y;
            spriteWidth = spriteRenderer.bounds.size.x;
        }

        // Check if using a material with _MainTex offset (for tiled backgrounds)
        Renderer rend = GetComponent<Renderer>();
        if (rend != null && rend.material != null && rend.material.HasProperty("_MainTex"))
        {
            mat = rend.material;
            useMaterialScroll = false; // Default to transform-based scrolling
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

        if (useMaterialScroll && mat != null)
        {
            MaterialScroll();
        }
        else
        {
            TransformScroll();
        }
    }

    private void TransformScroll()
    {
        float speed = scrollSpeed * parallaxFactor;

        if (scrollHorizontal)
        {
            transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
        }
        else
        {
            transform.Translate(Vector3.down * speed * Time.deltaTime, Space.World);
        }

        // Loop the background
        if (enableLooping)
        {
            if (!scrollHorizontal && transform.position.y <= startPositionY)
            {
                Vector3 pos = transform.position;
                pos.y = resetPositionY;
                transform.position = pos;
            }
            else if (scrollHorizontal && transform.position.x <= -spriteWidth)
            {
                Vector3 pos = transform.position;
                pos.x = spriteWidth;
                transform.position = pos;
            }
        }
    }

    private void MaterialScroll()
    {
        float speed = scrollSpeed * parallaxFactor;
        Vector2 offset = mat.mainTextureOffset;

        if (scrollHorizontal)
        {
            offset.x += speed * Time.deltaTime;
        }
        else
        {
            offset.y += speed * Time.deltaTime;
        }

        offset.x %= 1f;
        offset.y %= 1f;
        mat.mainTextureOffset = offset;
    }

    /// <summary>
    /// Change scroll speed at runtime.
    /// </summary>
    public void SetScrollSpeed(float newSpeed)
    {
        scrollSpeed = newSpeed;
    }
}
