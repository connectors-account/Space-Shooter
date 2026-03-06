using UnityEngine;

/// <summary>
/// Creates a scrolling parallax background effect.
/// Supports multiple layers with different scroll speeds for depth.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public SpriteRenderer spriteRenderer;
        public float scrollSpeed = 1f;
        public bool tile = true;
    }
    
    [Header("Parallax Settings")]
    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private float baseScrollSpeed = 2f;
    [SerializeField] private bool autoGenerate = true;
    
    [Header("Auto-Generated Background Settings")]
    [SerializeField] private int starCount = 100;
    [SerializeField] private int layerCount = 3;
    [SerializeField] private Color backgroundColor = new Color(0.02f, 0.02f, 0.08f);
    
    // Internal state for auto-generated backgrounds
    private GameObject[] backgroundLayers;
    private Material[] layerMaterials;
    private float[] layerOffsets;
    
    private void Start()
    {
        if (autoGenerate)
        {
            GenerateSpaceBackground();
        }
        else if (layers != null && layers.Length > 0)
        {
            InitializeLayers();
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        
        if (autoGenerate && layerMaterials != null)
        {
            UpdateAutoBackground();
        }
        else if (layers != null)
        {
            UpdateManualLayers();
        }
    }
    
    /// <summary>
    /// Generate procedural space background with stars
    /// </summary>
    private void GenerateSpaceBackground()
    {
        backgroundLayers = new GameObject[layerCount];
        layerMaterials = new Material[layerCount];
        layerOffsets = new float[layerCount];
        
        // Create background color
        Camera.main.backgroundColor = backgroundColor;
        
        for (int i = 0; i < layerCount; i++)
        {
            // Create layer object
            GameObject layer = new GameObject($"StarLayer_{i}");
            layer.transform.SetParent(transform);
            layer.transform.localPosition = new Vector3(0, 0, i + 1);
            
            // Create sprite
            SpriteRenderer sr = layer.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStarFieldSprite(starCount / (i + 1), i);
            sr.sortingOrder = -100 + i;
            
            // Scale to cover screen
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(20f, 30f);
            
            backgroundLayers[i] = layer;
            layerOffsets[i] = 0f;
        }
    }
    
    /// <summary>
    /// Create star field sprite texture
    /// </summary>
    private Sprite CreateStarFieldSprite(int stars, int layerIndex)
    {
        int resolution = 256;
        Texture2D texture = new Texture2D(resolution, resolution);
        texture.wrapMode = TextureWrapMode.Repeat;
        
        // Fill with transparent
        Color[] pixels = new Color[resolution * resolution];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        texture.SetPixels(pixels);
        
        // Add stars
        System.Random rng = new System.Random(layerIndex * 12345);
        for (int i = 0; i < stars; i++)
        {
            int x = rng.Next(resolution);
            int y = rng.Next(resolution);
            
            // Star brightness based on layer (closer = brighter)
            float brightness = 0.3f + (layerCount - layerIndex) * 0.2f;
            brightness *= (float)rng.NextDouble() * 0.5f + 0.5f;
            
            // Star size (1-3 pixels)
            int size = rng.Next(1, 3 - layerIndex / 2);
            
            Color starColor = new Color(brightness, brightness, brightness * 1.1f, 1f);
            
            for (int dx = -size; dx <= size; dx++)
            {
                for (int dy = -size; dy <= size; dy++)
                {
                    int px = (x + dx + resolution) % resolution;
                    int py = (y + dy + resolution) % resolution;
                    
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= size)
                    {
                        float alpha = 1f - dist / (size + 1);
                        Color existing = texture.GetPixel(px, py);
                        texture.SetPixel(px, py, Color.Lerp(existing, starColor, alpha));
                    }
                }
            }
        }
        
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, resolution, resolution), Vector2.one * 0.5f, resolution / 10f);
    }
    
    private void UpdateAutoBackground()
    {
        for (int i = 0; i < backgroundLayers.Length; i++)
        {
            if (backgroundLayers[i] == null) continue;
            
            // Different speed for each layer (parallax effect)
            float layerSpeed = baseScrollSpeed * (1f / (i + 1));
            layerOffsets[i] += layerSpeed * Time.deltaTime;
            
            // Wrap offset
            if (layerOffsets[i] > 30f)
            {
                layerOffsets[i] -= 30f;
            }
            
            // Move layer
            Vector3 pos = backgroundLayers[i].transform.localPosition;
            pos.y = -layerOffsets[i];
            backgroundLayers[i].transform.localPosition = pos;
        }
    }
    
    private void InitializeLayers()
    {
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer != null && layer.tile)
            {
                layer.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
            }
        }
    }
    
    private void UpdateManualLayers()
    {
        foreach (var layer in layers)
        {
            if (layer.spriteRenderer == null) continue;
            
            Vector3 pos = layer.spriteRenderer.transform.localPosition;
            pos.y -= layer.scrollSpeed * baseScrollSpeed * Time.deltaTime;
            
            // Reset position when scrolled too far
            if (pos.y < -20f)
            {
                pos.y += 40f;
            }
            
            layer.spriteRenderer.transform.localPosition = pos;
        }
    }
    
    /// <summary>
    /// Set base scroll speed
    /// </summary>
    public void SetScrollSpeed(float speed)
    {
        baseScrollSpeed = speed;
    }
    
    /// <summary>
    /// Pause scrolling
    /// </summary>
    public void PauseScrolling()
    {
        enabled = false;
    }
    
    /// <summary>
    /// Resume scrolling
    /// </summary>
    public void ResumeScrolling()
    {
        enabled = true;
    }
}
