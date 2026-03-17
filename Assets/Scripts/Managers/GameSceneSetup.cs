using UnityEngine;

/// <summary>
/// Bootstrapper for the Game scene. Creates all required GameObjects at runtime
/// so the scene file can be minimal. Attach to an empty root GameObject.
/// </summary>
public class GameSceneSetup : MonoBehaviour
{
    [Header("Prefab References (assign if available, or leave null for runtime creation)")]
    public GameObject playerPrefab;
    public GameObject bulletPrefab;
    public GameObject basicEnemyPrefab;
    public GameObject fastEnemyPrefab;
    public GameObject tankyEnemyPrefab;
    public GameObject explosionPrefab;
    public GameObject rapidFirePowerUpPrefab;
    public GameObject shieldPowerUpPrefab;
    public GameObject healthPowerUpPrefab;

    void Awake()
    {
        SetupCamera();
        SetupBackground();
        SetupPlayer();
        SetupSpawner();
        SetupUI();
        SetupAudio();
        SetupGameManager();
    }

    void SetupCamera()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 6;
        Camera.main.backgroundColor = new Color(0.03f, 0.01f, 0.1f);
    }

    void SetupBackground()
    {
        GameObject bg = new GameObject("Background");
        BackgroundSetup bgSetup = bg.AddComponent<BackgroundSetup>();
        bgSetup.scrollSpeed = 2f;
    }

    void SetupPlayer()
    {
        GameObject player;
        if (playerPrefab != null)
        {
            player = Instantiate(playerPrefab);
        }
        else
        {
            player = CreateRuntimePlayer();
        }
        player.transform.position = new Vector3(0, -4, 0);
    }

    GameObject CreateRuntimePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");

        // Sprite
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = CreateTriangleSprite(Color.cyan, "PlayerSprite");
        sr.sortingOrder = 5;

        // Collider
        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.7f);

        // Rigidbody for collision detection
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // Player controller
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.bulletPrefab = GetOrCreateBulletPrefab(true);
        pc.explosionPrefab = GetOrCreateExplosionPrefab();
        pc.shieldVisualPrefab = CreateShieldVisual();

        return player;
    }

    void SetupSpawner()
    {
        GameObject spawnerObj = new GameObject("EnemySpawner");
        EnemySpawner spawner = spawnerObj.AddComponent<EnemySpawner>();

        spawner.basicEnemyPrefab = GetOrCreateEnemyPrefab("BasicEnemy", Color.red, 1);
        spawner.fastEnemyPrefab = GetOrCreateEnemyPrefab("FastEnemy", Color.yellow, 2);
        spawner.tankyEnemyPrefab = GetOrCreateEnemyPrefab("TankyEnemy", Color.magenta, 3);
    }

    void SetupUI()
    {
        GameObject uiHelper = new GameObject("UISetupHelper");
        uiHelper.AddComponent<UISetupHelper>();
    }

    void SetupAudio()
    {
        if (AudioManager.Instance != null) return;
        GameObject audioObj = new GameObject("AudioManager");
        audioObj.AddComponent<AudioManager>();
    }

    void SetupGameManager()
    {
        if (GameManager.Instance != null) return;
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
    }

    // === Runtime Prefab Creation ===

    GameObject GetOrCreateBulletPrefab(bool isPlayer)
    {
        if (bulletPrefab != null) return bulletPrefab;

        GameObject bullet = new GameObject(isPlayer ? "PlayerBullet" : "EnemyBullet");
        bullet.SetActive(false); // deactivate template

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = CreateRectSprite(isPlayer ? Color.green : Color.red, "BulletSprite");
        sr.sortingOrder = 3;
        bullet.transform.localScale = new Vector3(0.15f, 0.4f, 1f);

        BoxCollider2D col = bullet.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        bullet.AddComponent<Bullet>();

        // We need to keep this as an inactive template and return it
        bullet.SetActive(true);
        DontDestroyOnLoad(bullet);
        bullet.SetActive(false);

        return bullet;
    }

    GameObject GetOrCreateEnemyPrefab(string enemyType, Color color, int tier)
    {
        GameObject enemy = new GameObject(enemyType);
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        float scale = 0.5f + (tier - 1) * 0.15f;
        sr.sprite = CreateDiamondSprite(color, $"{enemyType}Sprite");
        sr.sortingOrder = 4;
        enemy.transform.localScale = Vector3.one * scale;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        // Add type-specific component
        switch (enemyType)
        {
            case "BasicEnemy":
                BasicEnemy be = enemy.AddComponent<BasicEnemy>();
                be.bulletPrefab = GetOrCreateBulletPrefab(false);
                be.explosionPrefab = GetOrCreateExplosionPrefab();
                be.possibleDrops = CreatePowerUpPrefabs();
                break;
            case "FastEnemy":
                FastEnemy fe = enemy.AddComponent<FastEnemy>();
                fe.bulletPrefab = GetOrCreateBulletPrefab(false);
                fe.explosionPrefab = GetOrCreateExplosionPrefab();
                fe.possibleDrops = CreatePowerUpPrefabs();
                break;
            case "TankyEnemy":
                TankyEnemy te = enemy.AddComponent<TankyEnemy>();
                te.bulletPrefab = GetOrCreateBulletPrefab(false);
                te.explosionPrefab = GetOrCreateExplosionPrefab();
                te.possibleDrops = CreatePowerUpPrefabs();
                break;
        }

        DontDestroyOnLoad(enemy);
        enemy.SetActive(false);
        return enemy;
    }

    GameObject GetOrCreateExplosionPrefab()
    {
        if (explosionPrefab != null) return explosionPrefab;

        GameObject explosion = new GameObject("Explosion");
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(1f, 0.5f, 0f), "ExplosionSprite");
        sr.sortingOrder = 10;
        explosion.AddComponent<Explosion>();

        DontDestroyOnLoad(explosion);
        explosion.SetActive(false);
        return explosion;
    }

    GameObject[] CreatePowerUpPrefabs()
    {
        return new GameObject[]
        {
            CreatePowerUpPrefab(PowerUp.PowerUpType.RapidFire, Color.yellow, "RapidFire"),
            CreatePowerUpPrefab(PowerUp.PowerUpType.Shield, new Color(0.3f, 0.5f, 1f), "Shield"),
            CreatePowerUpPrefab(PowerUp.PowerUpType.HealthRestore, Color.green, "Health")
        };
    }

    GameObject CreatePowerUpPrefab(PowerUp.PowerUpType type, Color color, string name)
    {
        GameObject pu = new GameObject($"PowerUp_{name}");
        pu.tag = "Untagged"; // Power-ups use trigger collision with Player tag check

        SpriteRenderer sr = pu.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(color, $"PU_{name}_Sprite");
        sr.sortingOrder = 6;
        pu.transform.localScale = Vector3.one * 0.3f;

        BoxCollider2D col = pu.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.5f, 1.5f);

        Rigidbody2D rb = pu.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        PowerUp puScript = pu.AddComponent<PowerUp>();
        puScript.type = type;

        DontDestroyOnLoad(pu);
        pu.SetActive(false);
        return pu;
    }

    GameObject CreateShieldVisual()
    {
        GameObject shield = new GameObject("ShieldVisual");
        SpriteRenderer sr = shield.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite(new Color(0.3f, 0.5f, 1f, 0.4f), "ShieldSprite");
        sr.sortingOrder = 7;
        shield.transform.localScale = Vector3.one * 1.5f;

        DontDestroyOnLoad(shield);
        shield.SetActive(false);
        return shield;
    }

    // === Procedural Sprite Generation ===

    Sprite CreateTriangleSprite(Color color, string name)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;
        Color clear = Color.clear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Triangle pointing up
                float normalY = (float)y / size;
                float halfWidth = (1f - normalY) * 0.5f;
                float normalX = (float)x / size - 0.5f;

                if (Mathf.Abs(normalX) <= halfWidth)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
    }

    Sprite CreateDiamondSprite(Color color, string name)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (float)x / size - 0.5f;
                float ny = (float)y / size - 0.5f;
                if (Mathf.Abs(nx) + Mathf.Abs(ny) < 0.45f)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
    }

    Sprite CreateRectSprite(Color color, string name)
    {
        Texture2D tex = new Texture2D(8, 16);
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 8; x++)
                tex.SetPixel(x, y, color);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 16), new Vector2(0.5f, 0.5f), 16);
    }

    Sprite CreateCircleSprite(Color color, string name)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist < radius - 1)
                    tex.SetPixel(x, y, color);
                else
                    tex.SetPixel(x, y, Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 64);
    }
}
