using UnityEngine;

/// <summary>
/// Scrolls a tiled background sprite downward at a configurable speed,
/// looping seamlessly.  Attach to a quad/sprite with a tiling material
/// OR use the built-in texture offset approach.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 0.5f;
    [SerializeField] private bool  useRendererOffset = false;

    // For the transform-based approach we need two sprites stacked
    [Header("Transform-Based (assign both children)")]
    [SerializeField] private Transform bgPanel1;
    [SerializeField] private Transform bgPanel2;
    [SerializeField] private float     panelHeight = 10f;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();

        // Auto-detect children if not assigned
        if (bgPanel1 == null && transform.childCount >= 2)
        {
            bgPanel1 = transform.GetChild(0);
            bgPanel2 = transform.GetChild(1);
        }

        // Calculate panel height from sprite bounds if possible
        if (bgPanel1 != null)
        {
            SpriteRenderer sr = bgPanel1.GetComponent<SpriteRenderer>();
            if (sr != null) panelHeight = sr.bounds.size.y;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

        if (useRendererOffset && rend != null)
        {
            // Material offset scrolling (needs tiling material)
            Vector2 offset = rend.material.mainTextureOffset;
            offset.y += scrollSpeed * Time.deltaTime;
            rend.material.mainTextureOffset = offset;
        }
        else if (bgPanel1 != null && bgPanel2 != null)
        {
            // Transform-based scrolling
            bgPanel1.position += Vector3.down * scrollSpeed * Time.deltaTime;
            bgPanel2.position += Vector3.down * scrollSpeed * Time.deltaTime;

            if (bgPanel1.position.y <= -panelHeight)
                bgPanel1.position = new Vector3(bgPanel1.position.x, bgPanel2.position.y + panelHeight, bgPanel1.position.z);

            if (bgPanel2.position.y <= -panelHeight)
                bgPanel2.position = new Vector3(bgPanel2.position.x, bgPanel1.position.y + panelHeight, bgPanel2.position.z);
        }
    }
}
