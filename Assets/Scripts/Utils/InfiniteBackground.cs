using UnityEngine;

/// <summary>
/// InfiniteBackground creates a seamlessly scrolling infinite background
/// using two tiled sprites that swap positions.
/// </summary>
public class InfiniteBackground : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private float scrollSpeed = 2f;
    [SerializeField] private Sprite backgroundSprite;

    [Header("Star Field Settings")]
    [SerializeField] private bool generateStarField = true;
    [SerializeField] private int starCount = 100;
    [SerializeField] private Color starColor = Color.white;

    // Components
    private SpriteRenderer[] backgrounds;
    private float backgroundHeight;
    private int backgroundCount = 2;

    private void Start()
    {
        SetupBackgrounds();
    }

    /// <summary>
    /// Setup the scrolling backgrounds
    /// </summary>
    private void SetupBackgrounds()
    {
        backgrounds = new SpriteRenderer[backgroundCount];

        // Get or create sprite renderer
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            backgroundSprite = sr.sprite;
            backgroundHeight = sr.bounds.size.y;
        }
        else
        {
            // Create default background
            backgroundHeight = 12f; // Default height
        }

        // Create background tiles
        for (int i = 0; i < backgroundCount; i++)
        {
            GameObject bgObj = new GameObject($"Background_{i}");
            bgObj.transform.SetParent(transform);
            bgObj.transform.localPosition = new Vector3(0, i * backgroundHeight, 0);

            SpriteRenderer bgRenderer = bgObj.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = backgroundSprite;
            bgRenderer.sortingLayerName = "Background";
            bgRenderer.sortingOrder = -100;

            backgrounds[i] = bgRenderer;

            // Generate star field if enabled
            if (generateStarField)
            {
                CreateStarField(bgObj.transform);
            }
        }

        // Disable original sprite renderer if exists
        if (sr != null)
        {
            sr.enabled = false;
        }
    }

    /// <summary>
    /// Create a procedural star field
    /// </summary>
    private void CreateStarField(Transform parent)
    {
        GameObject starContainer = new GameObject("Stars");
        starContainer.transform.SetParent(parent);
        starContainer.transform.localPosition = Vector3.zero;

        for (int i = 0; i < starCount; i++)
        {
            GameObject star = new GameObject($"Star_{i}");
            star.transform.SetParent(starContainer.transform);
            
            // Random position within background bounds
            float x = Random.Range(-9f, 9f);
            float y = Random.Range(-backgroundHeight / 2f, backgroundHeight / 2f);
            star.transform.localPosition = new Vector3(x, y, 0);

            // Add sprite renderer
            SpriteRenderer starRenderer = star.AddComponent<SpriteRenderer>();
            starRenderer.color = starColor * Random.Range(0.5f, 1f);
            starRenderer.sortingLayerName = "Background";
            starRenderer.sortingOrder = -99;

            // Random size
            float size = Random.Range(0.02f, 0.08f);
            star.transform.localScale = Vector3.one * size;
        }
    }

    private void Update()
    {
        // Don't scroll if game is paused
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
            return;

        ScrollBackgrounds();
    }

    /// <summary>
    /// Scroll and reposition backgrounds for infinite effect
    /// </summary>
    private void ScrollBackgrounds()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (backgrounds[i] == null) continue;

            // Move down
            Vector3 pos = backgrounds[i].transform.localPosition;
            pos.y -= scrollSpeed * Time.deltaTime;

            // Check if off screen and needs repositioning
            if (pos.y <= -backgroundHeight)
            {
                // Find the highest background
                float highestY = float.MinValue;
                foreach (var bg in backgrounds)
                {
                    if (bg != null && bg.transform.localPosition.y > highestY)
                    {
                        highestY = bg.transform.localPosition.y;
                    }
                }
                
                // Position above the highest
                pos.y = highestY + backgroundHeight;
            }

            backgrounds[i].transform.localPosition = pos;
        }
    }

    /// <summary>
    /// Set scroll speed
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        scrollSpeed = speed;
    }
}
