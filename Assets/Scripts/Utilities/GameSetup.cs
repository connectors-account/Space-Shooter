using UnityEngine;

/// <summary>
/// GameSetup creates placeholder game objects at runtime.
/// This is useful when you don't have the scene fully set up in the editor.
/// Attach this to a single empty GameObject in the scene.
/// </summary>
public class GameSetup : MonoBehaviour
{
    [Header("Setup Options")]
    [Tooltip("Create placeholder sprites if prefabs are missing")]
    public bool createPlaceholders = true;

    [Header("Prefab References (optional)")]
    public GameObject playerPrefab;
    public GameObject basicEnemyPrefab;
    public GameObject zigzagEnemyPrefab;
    public GameObject diveBomberPrefab;
    public GameObject bulletPrefab;
    public GameObject powerUpPrefab;

    void Awake()
    {
        if (createPlaceholders)
        {
            SetupPlaceholders();
        }
    }

    /// <summary>
    /// Create placeholder objects if they don't exist
    /// </summary>
    void SetupPlaceholders()
    {
        // Create player if not found
        if (GameObject.FindGameObjectWithTag("Player") == null && playerPrefab == null)
        {
            CreatePlayer();
        }
        
        // Create bullet prefab
        if (bulletPrefab == null)
        {
            bulletPrefab = CreateBulletPrefab();
        }
        
        // Create enemy prefabs
        if (basicEnemyPrefab == null)
        {
            basicEnemyPrefab = CreateBasicEnemyPrefab();
        }
        
        if (zigzagEnemyPrefab == null)
        {
            zigzagEnemyPrefab = CreateZigZagEnemyPrefab();
        }
        
        if (diveBomberPrefab == null)
        {
            diveBomberPrefab = CreateDiveBomberPrefab();
        }
        
        if (powerUpPrefab == null)
        {
            powerUpPrefab = CreatePowerUpPrefab();
        }
        
        // Assign prefabs to spawner
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            if (spawner.basicEnemyPrefab == null) spawner.basicEnemyPrefab = basicEnemyPrefab;
            if (spawner.zigzagEnemyPrefab == null) spawner.zigzagEnemyPrefab = zigzagEnemyPrefab;
            if (spawner.diveBomberPrefab == null) spawner.diveBomberPrefab = diveBomberPrefab;
        }
        
        // Assign prefabs to power-up spawner
        PowerUpSpawner puSpawner = FindObjectOfType<PowerUpSpawner>();
        if (puSpawner != null)
        {
            if (puSpawner.healthPowerUpPrefab == null) puSpawner.healthPowerUpPrefab = powerUpPrefab;
        }
        
        // Assign bullet prefab to player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null && pc.bulletPrefab == null)
            {
                pc.bulletPrefab = bulletPrefab;
            }
        }
    }

    /// <summary>
    /// Create a placeholder player object
    /// </summary>
    void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3, 0);
        
        // Add sprite renderer with placeholder
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(Color.cyan, 0.5f, 0.7f);
        sr.sortingOrder = 10;
        
        // Add collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 0.7f);
        col.isTrigger = true;
        
        // Add rigidbody
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Add scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
    }

    /// <summary>
    /// Create bullet prefab
    /// </summary>
    GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BulletPrefab");
        bullet.tag = "PlayerBullet";
        
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(Color.yellow, 0.1f, 0.3f);
        sr.sortingOrder = 5;
        
        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.1f, 0.3f);
        col.isTrigger = true;
        
        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        bullet.AddComponent<Bullet>();
        
        bullet.SetActive(false);
        DontDestroyOnLoad(bullet);
        
        return bullet;
    }

    /// <summary>
    /// Create basic enemy prefab
    /// </summary>
    GameObject CreateBasicEnemyPrefab()
    {
        GameObject enemy = new GameObject("BasicEnemyPrefab");
        enemy.tag = "Enemy";
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(Color.red, 0.5f, 0.5f);
        sr.sortingOrder = 9;
        
        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 0.5f);
        col.isTrigger = true;
        
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        BasicEnemy script = enemy.AddComponent<BasicEnemy>();
        script.bulletPrefab = bulletPrefab;
        
        enemy.SetActive(false);
        DontDestroyOnLoad(enemy);
        
        return enemy;
    }

    /// <summary>
    /// Create zigzag enemy prefab
    /// </summary>
    GameObject CreateZigZagEnemyPrefab()
    {
        GameObject enemy = new GameObject("ZigZagEnemyPrefab");
        enemy.tag = "Enemy";
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(Color.magenta, 0.6f, 0.4f);
        sr.sortingOrder = 9;
        
        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.6f, 0.4f);
        col.isTrigger = true;
        
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        ZigZagEnemy script = enemy.AddComponent<ZigZagEnemy>();
        script.bulletPrefab = bulletPrefab;
        
        enemy.SetActive(false);
        DontDestroyOnLoad(enemy);
        
        return enemy;
    }

    /// <summary>
    /// Create dive bomber enemy prefab
    /// </summary>
    GameObject CreateDiveBomberPrefab()
    {
        GameObject enemy = new GameObject("DiveBomberPrefab");
        enemy.tag = "Enemy";
        
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(new Color(1f, 0.5f, 0f), 0.7f, 0.5f); // Orange
        sr.sortingOrder = 9;
        
        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.7f, 0.5f);
        col.isTrigger = true;
        
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        DiveBomberEnemy script = enemy.AddComponent<DiveBomberEnemy>();
        script.bulletPrefab = bulletPrefab;
        
        enemy.SetActive(false);
        DontDestroyOnLoad(enemy);
        
        return enemy;
    }

    /// <summary>
    /// Create power-up prefab
    /// </summary>
    GameObject CreatePowerUpPrefab()
    {
        GameObject powerUp = new GameObject("PowerUpPrefab");
        powerUp.tag = "PowerUp";
        
        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePlaceholderSprite(Color.green, 0.4f, 0.4f);
        sr.sortingOrder = 8;
        
        CircleCollider2D col = powerUp.AddComponent<CircleCollider2D>();
        col.radius = 0.2f;
        col.isTrigger = true;
        
        Rigidbody2D rb = powerUp.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        powerUp.AddComponent<PowerUp>();
        
        powerUp.SetActive(false);
        DontDestroyOnLoad(powerUp);
        
        return powerUp;
    }

    /// <summary>
    /// Create a simple placeholder sprite (colored rectangle)
    /// </summary>
    Sprite CreatePlaceholderSprite(Color color, float width, float height)
    {
        int pixelWidth = Mathf.Max(1, Mathf.RoundToInt(width * 32));
        int pixelHeight = Mathf.Max(1, Mathf.RoundToInt(height * 32));
        
        Texture2D texture = new Texture2D(pixelWidth, pixelHeight);
        Color[] pixels = new Color[pixelWidth * pixelHeight];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, pixelWidth, pixelHeight), new Vector2(0.5f, 0.5f), 32f);
    }
}
