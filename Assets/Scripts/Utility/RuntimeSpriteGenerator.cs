using UnityEngine;

/// <summary>
/// Generates placeholder sprites at runtime when no sprite assets are assigned.
/// Attach to a GameObject in the scene — runs once in Awake.
/// This allows the game to run without pre-made sprite assets.
/// </summary>
public class RuntimeSpriteGenerator : MonoBehaviour
{
    public static RuntimeSpriteGenerator Instance { get; private set; }

    // Generated sprites accessible to other scripts
    public Sprite PlayerShipSprite { get; private set; }
    public Sprite EnemyBasicSprite { get; private set; }
    public Sprite EnemyFastSprite { get; private set; }
    public Sprite EnemyTankSprite { get; private set; }
    public Sprite EnemyShooterSprite { get; private set; }
    public Sprite PlayerBulletSprite { get; private set; }
    public Sprite EnemyBulletSprite { get; private set; }
    public Sprite PowerUpWeaponSprite { get; private set; }
    public Sprite PowerUpShieldSprite { get; private set; }
    public Sprite PowerUpHealthSprite { get; private set; }
    public Sprite StarSprite { get; private set; }
    public Sprite BackgroundSprite { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        GenerateAllSprites();
    }

    private void GenerateAllSprites()
    {
        PlayerShipSprite = CreateTriangle(32, 32, new Color(0.2f, 0.9f, 0.3f), true);
        EnemyBasicSprite = CreateTriangle(28, 28, new Color(0.9f, 0.2f, 0.2f), false);
        EnemyFastSprite = CreateDiamond(24, 24, new Color(1f, 0.6f, 0.1f));
        EnemyTankSprite = CreateRect(36, 36, new Color(0.6f, 0.1f, 0.1f));
        EnemyShooterSprite = CreateTriangle(30, 30, new Color(0.7f, 0.2f, 0.9f), false);
        PlayerBulletSprite = CreateRect(4, 12, new Color(1f, 1f, 0.5f));
        EnemyBulletSprite = CreateCircle(8, new Color(1f, 0.3f, 0.3f));
        PowerUpWeaponSprite = CreateRect(20, 20, new Color(1f, 1f, 0.2f));
        PowerUpShieldSprite = CreateCircle(20, new Color(0.3f, 0.8f, 1f));
        PowerUpHealthSprite = CreateCross(20, 20, new Color(0.2f, 1f, 0.3f));
        StarSprite = CreateCircle(4, Color.white);
        BackgroundSprite = CreateRect(64, 64, new Color(0.02f, 0.02f, 0.1f));
    }

    private Sprite CreateTriangle(int w, int h, Color color, bool pointUp)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < h; y++)
        {
            int row = pointUp ? y : (h - 1 - y);
            float progress = (float)row / h;
            int halfW = Mathf.RoundToInt(progress * w / 2f);
            int cx = w / 2;
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, (x >= cx - halfW && x <= cx + halfW) ? color : Color.clear);
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    private Sprite CreateRect(int w, int h, Color color)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = color;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    private Sprite CreateCircle(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        float c = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = ((x - c) * (x - c) + (y - c) * (y - c)) / (c * c);
                tex.SetPixel(x, y, d <= 1f ? color : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
    }

    private Sprite CreateDiamond(int w, int h, Color color)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        float cx = w / 2f, cy = h / 2f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - cx) / cx;
                float dy = Mathf.Abs(y - cy) / cy;
                tex.SetPixel(x, y, (dx + dy <= 1f) ? color : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    private Sprite CreateCross(int w, int h, Color color)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        float cx = w / 2f, cy = h / 2f, arm = w * 0.3f;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                bool hor = Mathf.Abs(y - cy) <= arm / 2f;
                bool ver = Mathf.Abs(x - cx) <= arm / 2f;
                tex.SetPixel(x, y, (hor || ver) ? color : Color.clear);
            }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }
}
