// ============================================================================
// BackgroundSetup.cs - Procedurally generates parallax background layers
// Creates star-field textures at runtime so no image assets are needed.
// ============================================================================
using UnityEngine;

/// <summary>
/// Generates 3 parallax layers of procedural star-field backgrounds on Start().
/// Attach to an empty GameObject in the Game scene. Each layer is given
/// different star density, brightness, and scroll speed for depth.
/// </summary>
public class BackgroundSetup : MonoBehaviour
{
    [Header("Background Generation")]
    [Tooltip("Pixels per unit for the generated textures.")]
    [SerializeField] private int pixelsPerUnit = 32;
    [Tooltip("Texture width in pixels.")]
    [SerializeField] private int textureWidth = 512;
    [Tooltip("Texture height in pixels.")]
    [SerializeField] private int textureHeight = 1024;

    [Header("Layer Configuration")]
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.08f, 1f);

    // Layer definitions: (scroll speed, star count, star brightness, star size).
    private readonly (float speed, int stars, float brightness, int maxSize)[] layers = new[]
    {
        (-0.5f, 60,  0.3f, 1),   // Far layer: dim, slow, small stars.
        (-1.5f, 100, 0.6f, 2),   // Mid layer: medium brightness and speed.
        (-3.0f, 40,  1.0f, 3),   // Near layer: bright, fast, larger stars.
    };

    // ========================================================================
    // Unity Lifecycle
    // ========================================================================

    private void Awake()
    {
        CreateLayers();
    }

    // ========================================================================
    // Layer Generation
    // ========================================================================

    /// <summary>
    /// Creates three parallax background layers with procedural star textures.
    /// Each layer is a child GameObject with a SpriteRenderer and ParallaxBackground component.
    /// Two copies of each layer are stacked vertically for seamless scrolling.
    /// </summary>
    private void CreateLayers()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            var (speed, starCount, brightness, maxSize) = layers[i];

            // Generate the star texture.
            Texture2D tex = GenerateStarTexture(starCount, brightness, maxSize);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);

            float spriteHeight = (float)textureHeight / pixelsPerUnit;

            // Create two copies for seamless vertical tiling.
            for (int copy = 0; copy < 2; copy++)
            {
                GameObject layer = new GameObject($"BG_Layer{i}_Copy{copy}");
                layer.transform.SetParent(transform);

                SpriteRenderer sr = layer.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = "Background";
                sr.sortingOrder = i; // Farther layers render behind closer ones.

                // Position: first copy at center, second copy directly above.
                float yPos = copy * spriteHeight;
                layer.transform.position = new Vector3(0f, yPos, 10f - i); // Z for depth ordering.

                ParallaxBackground parallax = layer.AddComponent<ParallaxBackground>();
                // Set scroll speed and sprite height via serialized fields using reflection
                // (or by setting them before Start runs, which works since we're in Awake).
                SetPrivateField(parallax, "scrollSpeed", speed);
                SetPrivateField(parallax, "spriteHeight", spriteHeight);
                SetPrivateField(parallax, "autoCalculateHeight", false);
            }
        }
    }

    /// <summary>
    /// Generates a procedural star-field texture.
    /// </summary>
    private Texture2D GenerateStarTexture(int starCount, float brightness, int maxSize)
    {
        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        // Fill with background color.
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int p = 0; p < pixels.Length; p++)
        {
            pixels[p] = backgroundColor;
        }

        // Scatter stars.
        for (int s = 0; s < starCount; s++)
        {
            int x = Random.Range(0, textureWidth);
            int y = Random.Range(0, textureHeight);
            int size = Random.Range(1, maxSize + 1);

            // Random star color: mostly white with occasional blue or yellow tint.
            Color starColor;
            float colorRoll = Random.value;
            if (colorRoll < 0.7f)
                starColor = Color.white;
            else if (colorRoll < 0.85f)
                starColor = new Color(0.7f, 0.8f, 1f); // Blue-ish.
            else
                starColor = new Color(1f, 1f, 0.7f); // Yellow-ish.

            starColor *= brightness;
            starColor.a = 1f;

            // Draw the star as a small square.
            for (int dx = 0; dx < size; dx++)
            {
                for (int dy = 0; dy < size; dy++)
                {
                    int px = Mathf.Clamp(x + dx, 0, textureWidth - 1);
                    int py = Mathf.Clamp(y + dy, 0, textureHeight - 1);
                    pixels[py * textureWidth + px] = starColor;
                }
            }
        }

        // Add a few nebula-like color patches for variety.
        for (int n = 0; n < 3; n++)
        {
            int cx = Random.Range(0, textureWidth);
            int cy = Random.Range(0, textureHeight);
            int radius = Random.Range(30, 80);
            Color nebulaColor = new Color(
                Random.Range(0.05f, 0.15f),
                Random.Range(0.02f, 0.1f),
                Random.Range(0.1f, 0.25f),
                1f
            );

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > radius) continue;

                    int px = Mathf.Clamp(cx + dx, 0, textureWidth - 1);
                    int py = Mathf.Clamp(cy + dy, 0, textureHeight - 1);
                    float falloff = 1f - (dist / radius);
                    falloff *= 0.3f * brightness;
                    Color existing = pixels[py * textureWidth + px];
                    pixels[py * textureWidth + px] = Color.Lerp(existing, nebulaColor, falloff);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// Helper to set a private serialized field via reflection (used during Awake before Start).
    /// </summary>
    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
