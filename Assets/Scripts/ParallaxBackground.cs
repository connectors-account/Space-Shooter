using UnityEngine;

/// <summary>
/// ParallaxBackground creates a continuously scrolling star-field background.
/// Uses two tiled sprites that cycle endlessly for a seamless scroll effect.
/// Attach to an empty GameObject; it will create the background sprites at runtime.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    // ── Settings ─────────────────────────────────────────────
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 1.5f;
    [SerializeField] private float resetPositionY = -12f;
    [SerializeField] private float startPositionYOffset = 12f;

    [Header("Sprites")]
    [SerializeField] private Sprite backgroundSprite;
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.08f);

    // ── Second Layer (optional parallax depth) ───────────────
    [Header("Second Layer (Stars)")]
    [SerializeField] private float secondLayerSpeed = 0.8f;
    [SerializeField] private Color secondLayerColor = new Color(0.05f, 0.05f, 0.15f);

    // ── Internal ─────────────────────────────────────────────
    private Transform[] bgPanels;
    private Transform[] starPanels;
    private float panelHeight;

    // ──────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ──────────────────────────────────────────────────────────

    private void Start()
    {
        panelHeight = 12f; // Approximate camera height in world units

        // Create background panels
        bgPanels = CreateScrollLayer("BG", backgroundColor, -5f, scrollSpeed);
        starPanels = CreateScrollLayer("Stars", secondLayerColor, -4f, secondLayerSpeed);
    }

    private void Update()
    {
        ScrollLayer(bgPanels, scrollSpeed);
        ScrollLayer(starPanels, secondLayerSpeed);
    }

    // ──────────────────────────────────────────────────────────
    // Layer Creation & Scrolling
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates two tiled panels that scroll downward and wrap around.
    /// </summary>
    private Transform[] CreateScrollLayer(string layerName, Color color, float zDepth, float speed)
    {
        Transform[] panels = new Transform[2];

        for (int i = 0; i < 2; i++)
        {
            GameObject panel = new GameObject(layerName + "_" + i);
            panel.transform.SetParent(transform);

            SpriteRenderer sr = panel.AddComponent<SpriteRenderer>();

            if (backgroundSprite != null)
            {
                sr.sprite = backgroundSprite;
            }
            else
            {
                // Create a simple colored quad as fallback
                sr.sprite = CreatePlaceholderSprite();
            }

            sr.color = color;
            sr.sortingOrder = (int)zDepth;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(20f, panelHeight);

            float yPos = (i == 0) ? 0f : panelHeight;
            panel.transform.position = new Vector3(0f, yPos, zDepth);
            panel.transform.localScale = Vector3.one;

            panels[i] = panel.transform;
        }

        return panels;
    }

    /// <summary>
    /// Move panels downward; when a panel moves below the threshold, wrap it above.
    /// </summary>
    private void ScrollLayer(Transform[] panels, float speed)
    {
        if (panels == null) return;

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].Translate(Vector3.down * speed * Time.deltaTime, Space.World);

            if (panels[i].position.y <= resetPositionY)
            {
                // Find the highest panel and place above it
                float highestY = float.MinValue;
                for (int j = 0; j < panels.Length; j++)
                {
                    if (panels[j].position.y > highestY)
                        highestY = panels[j].position.y;
                }

                Vector3 pos = panels[i].position;
                pos.y = highestY + panelHeight;
                panels[i].position = pos;
            }
        }
    }

    // ──────────────────────────────────────────────────────────
    // Placeholder Sprite
    // ──────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a simple 4x4 white texture sprite at runtime.
    /// Used when no background sprite asset is assigned.
    /// </summary>
    private Sprite CreatePlaceholderSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
