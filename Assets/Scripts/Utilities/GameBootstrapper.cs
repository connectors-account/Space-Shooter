using UnityEngine;

/// <summary>
/// Bootstrapper that sets up the game scene with placeholder objects when real prefabs are missing.
/// Place this on an empty GameObject in your GameScene.
/// It auto-creates a player, background, and configures the camera.
/// 
/// This helps you test the game before setting up proper prefabs.
/// Remove or disable once you have real prefabs wired up.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    [Header("References (assign if you have prefabs)")]
    public GameObject playerPrefab;
    public bool useBootstrapper = true;

    private void Awake()
    {
        if (!useBootstrapper) return;

        SetupCamera();
        SetupBackground();

        if (playerPrefab == null && PlayerController.Instance == null)
        {
            CreatePlaceholderPlayer();
        }
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f); // deep space blue
            cam.transform.position = new Vector3(0, 0, -10);
        }
    }

    private void SetupBackground()
    {
        // Create a simple starfield background
        GameObject bg = new GameObject("Background");
        bg.transform.position = new Vector3(0, 0, 5);
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();

        // Generate a dark space texture with stars
        int w = 256, h = 512;
        Texture2D tex = new Texture2D(w, h);
        Color spaceColor = new Color(0.02f, 0.02f, 0.08f);

        Color[] pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = spaceColor;

        // Add random stars
        for (int i = 0; i < 200; i++)
        {
            int x = Random.Range(0, w);
            int y = Random.Range(0, h);
            float brightness = Random.Range(0.5f, 1f);
            pixels[y * w + x] = new Color(brightness, brightness, brightness);
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32);
        sr.sortingOrder = -100;

        // Add scrolling
        ParallaxBackground scroll = bg.AddComponent<ParallaxBackground>();
        scroll.scrollSpeed = 0.5f;
        scroll.tileHeight = h / 32f;

        // Duplicate for seamless scrolling
        GameObject bg2 = Instantiate(bg);
        bg2.name = "Background2";
        bg2.transform.position = new Vector3(0, h / 32f, 5);
    }

    private void CreatePlaceholderPlayer()
    {
        // Create player ship
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = PlaceholderSpriteGenerator.CreateTriangleSprite(Color.cyan, 32);
        sr.sortingOrder = 10;

        // Collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        // Rigidbody for trigger detection
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Health
        HealthManager hm = player.AddComponent<HealthManager>();
        hm.maxHealth = 100;

        // Controller
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 8f;

        // Create fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.parent = player.transform;
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);
        pc.firePoint = firePoint.transform;

        // Create bullet prefab reference (runtime-generated)
        pc.bulletPrefab = CreatePlaceholderBullet(true);
        pc.tripleShotBulletPrefab = CreatePlaceholderBullet(true);

        // Shield visual
        GameObject shield = new GameObject("Shield");
        shield.transform.parent = player.transform;
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
        shieldSr.sprite = PlaceholderSpriteGenerator.CreateCircleSprite(
            new Color(0.3f, 0.6f, 1f, 0.4f), 48);
        shieldSr.sortingOrder = 11;
        shield.SetActive(false);
        pc.shieldVisual = shield;
    }

    private GameObject CreatePlaceholderBullet(bool isPlayer)
    {
        GameObject bullet = new GameObject(isPlayer ? "PlayerBullet" : "EnemyBullet");
        bullet.SetActive(false); // template

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = PlaceholderSpriteGenerator.CreateCircleSprite(
            isPlayer ? Color.yellow : Color.red, 8);
        sr.sortingOrder = 5;

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.3f, 0.3f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Bullet b = bullet.AddComponent<Bullet>();
        b.isPlayerBullet = isPlayer;
        b.speed = isPlayer ? 12f : 6f;
        b.damage = isPlayer ? 25 : 20;
        b.rotateToDirection = true;

        // We need to keep this as a "pseudo-prefab" in the scene
        // In a real setup, this would be a proper prefab
        DontDestroyOnLoad(bullet);

        return bullet;
    }
}
