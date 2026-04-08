using UnityEngine;

/// <summary>
/// Generates all game sprites procedurally at runtime.
/// Attach to a GameObject in every scene (or use from a static call).
/// This eliminates the need for external sprite assets.
/// </summary>
public class SpriteGenerator : MonoBehaviour
{
    public static SpriteGenerator Instance { get; private set; }

    // Cached sprites
    public Sprite PlayerShip    { get; private set; }
    public Sprite EnemyBasic    { get; private set; }
    public Sprite EnemySine     { get; private set; }
    public Sprite EnemyShooter  { get; private set; }
    public Sprite BulletPlayer  { get; private set; }
    public Sprite BulletEnemy   { get; private set; }
    public Sprite PowerUpShield { get; private set; }
    public Sprite PowerUpRapid  { get; private set; }
    public Sprite PowerUpHealth { get; private set; }
    public Sprite BackgroundTile { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GenerateAll();
    }

    void GenerateAll()
    {
        PlayerShip     = CreatePlayerShipSprite();
        EnemyBasic     = CreateEnemySprite(new Color(1f, 0.3f, 0.3f), false);
        EnemySine      = CreateEnemySprite(new Color(1f, 0.6f, 0.1f), true);
        EnemyShooter   = CreateEnemyShooterSprite();
        BulletPlayer   = CreateBulletSprite(new Color(0.3f, 1f, 0.5f), 4, 10);
        BulletEnemy    = CreateBulletSprite(new Color(1f, 0.3f, 0.3f), 4, 8);
        PowerUpShield  = CreatePowerUpSprite(new Color(0.3f, 0.6f, 1f));
        PowerUpRapid   = CreatePowerUpSprite(new Color(1f, 1f, 0.2f));
        PowerUpHealth  = CreatePowerUpSprite(new Color(0.2f, 1f, 0.3f));
    }

    // ---- Sprite creation helpers ----

    Sprite CreatePlayerShipSprite()
    {
        // 32x32 pixel art player ship (arrow/triangle shape)
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color hull = new Color(0.2f, 0.6f, 1f);
        Color cockpit = new Color(0.5f, 0.9f, 1f);
        Color engine = new Color(1f, 0.5f, 0.1f);

        // Clear
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, clear);

        // Draw ship body (triangle pointing up)
        for (int y = 0; y < h; y++)
        {
            float ratio = (float)y / h;
            int halfWidth = (int)(ratio * (w / 2 - 2));
            int cx = w / 2;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < w)
                {
                    if (y < 4)
                        tex.SetPixel(x, y, engine); // engine glow
                    else if (y > h - 6 && Mathf.Abs(x - cx) < 3)
                        tex.SetPixel(x, y, cockpit); // cockpit
                    else
                        tex.SetPixel(x, y, hull);
                }
            }
        }

        // Wing details
        for (int y = 4; y < 14; y++)
        {
            tex.SetPixel(2, y, hull);
            tex.SetPixel(3, y, hull);
            tex.SetPixel(w - 3, y, hull);
            tex.SetPixel(w - 4, y, hull);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    Sprite CreateEnemySprite(Color color, bool hasWings)
    {
        int w = 28, h = 28;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color dark = color * 0.6f; dark.a = 1f;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, clear);

        // Inverted triangle (pointing down)
        for (int y = 0; y < h; y++)
        {
            float ratio = 1f - (float)y / h;
            int halfWidth = (int)(ratio * (w / 2 - 2));
            int cx = w / 2;
            for (int x = cx - halfWidth; x <= cx + halfWidth; x++)
            {
                if (x >= 0 && x < w)
                {
                    tex.SetPixel(x, y, (Mathf.Abs(x - cx) < 3) ? dark : color);
                }
            }
        }

        if (hasWings)
        {
            for (int y = h - 10; y < h - 2; y++)
            {
                tex.SetPixel(1, y, color);
                tex.SetPixel(2, y, color);
                tex.SetPixel(w - 2, y, color);
                tex.SetPixel(w - 3, y, color);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 28f);
    }

    Sprite CreateEnemyShooterSprite()
    {
        int w = 32, h = 32;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);
        Color body = new Color(0.7f, 0.2f, 0.8f);
        Color cannon = new Color(1f, 0.2f, 0.2f);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                tex.SetPixel(x, y, clear);

        // Hexagonal body
        for (int y = 4; y < h - 4; y++)
        {
            int halfW = (y < h / 2) ? (y - 2) : (h - y - 2);
            halfW = Mathf.Clamp(halfW, 2, w / 2 - 2);
            int cx = w / 2;
            for (int x = cx - halfW; x <= cx + halfW; x++)
                if (x >= 0 && x < w)
                    tex.SetPixel(x, y, body);
        }

        // Cannons on sides
        for (int y = 0; y < 8; y++)
        {
            tex.SetPixel(6, y, cannon);
            tex.SetPixel(7, y, cannon);
            tex.SetPixel(w - 7, y, cannon);
            tex.SetPixel(w - 8, y, cannon);
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
    }

    Sprite CreateBulletSprite(Color color, int w, int h)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color glow = color * 1.5f; glow.a = 1f;

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float dist = Mathf.Abs(x - w / 2f) / (w / 2f);
                tex.SetPixel(x, y, Color.Lerp(glow, color, dist));
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 16f);
    }

    Sprite CreatePowerUpSprite(Color color)
    {
        int w = 20, h = 20;
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        Color clear = new Color(0, 0, 0, 0);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(w / 2f, h / 2f));
                if (dist < w / 2f - 1)
                    tex.SetPixel(x, y, Color.Lerp(Color.white, color, dist / (w / 2f)));
                else
                    tex.SetPixel(x, y, clear);
            }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 20f);
    }
}
