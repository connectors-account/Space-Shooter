using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Auto-setup Settings")]
    public bool autoSetupOnStart = true;

    private static bool hasSetup = false;

    void Awake()
    {
        if (autoSetupOnStart && !hasSetup)
        {
            SetupGame();
            hasSetup = true;
        }
    }

    public void SetupGame()
    {
        SetupTags();
        SetupPlayer();
        SetupManagers();
        SetupPrefabs();
        SetupCamera();
        SetupBackground();

        Debug.Log("Game setup complete!");
    }

    void SetupTags()
    {
        Debug.Log("Note: Please ensure the following tags exist in Unity:");
        Debug.Log("- Player");
        Debug.Log("- Enemy");
        Debug.Log("- Bullet");
        Debug.Log("- PowerUp");
    }

    void SetupPlayer()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0, -3f, 0);

            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteGenerator.CreatePlayerSprite();
            sr.sortingOrder = 5;

            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.8f, 0.8f);

            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            PlayerController pc = player.AddComponent<PlayerController>();

            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(player.transform);
            firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);
            pc.firePoint = firePoint.transform;

            Debug.Log("Player created");
        }
    }

    void SetupManagers()
    {
        if (FindObjectOfType<GameManager>() == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
            Debug.Log("GameManager created");
        }

        if (FindObjectOfType<SpawnManager>() == null)
        {
            GameObject smObj = new GameObject("SpawnManager");
            smObj.AddComponent<SpawnManager>();
            Debug.Log("SpawnManager created");
        }

        if (FindObjectOfType<UIManager>() == null)
        {
            GameObject uiObj = new GameObject("UIManager");
            uiObj.AddComponent<UIManager>();
            Debug.Log("UIManager created");
        }

        if (FindObjectOfType<AudioManager>() == null)
        {
            GameObject amObj = new GameObject("AudioManager");
            amObj.AddComponent<AudioManager>();
            Debug.Log("AudioManager created");
        }
    }

    void SetupPrefabs()
    {
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        PlayerController player = FindObjectOfType<PlayerController>();

        if (spawnManager != null)
        {
            if (spawnManager.enemyPrefab == null)
            {
                spawnManager.enemyPrefab = CreateEnemyPrefab();
            }

            if (spawnManager.powerUpPrefab == null)
            {
                spawnManager.powerUpPrefab = CreatePowerUpPrefab();
            }
        }

        if (player != null && player.bulletPrefab == null)
        {
            player.bulletPrefab = CreateBulletPrefab();
        }

        EnemyController[] enemies = FindObjectsOfType<EnemyController>();
        foreach (var enemy in enemies)
        {
            if (enemy.bulletPrefab == null)
            {
                enemy.bulletPrefab = CreateBulletPrefab();
            }
        }
    }

    GameObject CreateEnemyPrefab()
    {
        GameObject enemy = new GameObject("EnemyPrefab");
        enemy.tag = "Enemy";

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateEnemySprite();
        sr.sortingOrder = 4;

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.9f, 0.9f);

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        EnemyController ec = enemy.AddComponent<EnemyController>();
        ec.bulletPrefab = CreateBulletPrefab();

        enemy.SetActive(false);
        return enemy;
    }

    GameObject CreateBulletPrefab()
    {
        GameObject bullet = new GameObject("BulletPrefab");
        bullet.tag = "Bullet";

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBulletSprite();
        sr.sortingOrder = 3;

        BoxCollider2D collider = bullet.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.2f, 0.5f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        bullet.AddComponent<BulletController>();

        bullet.SetActive(false);
        return bullet;
    }

    GameObject CreatePowerUpPrefab()
    {
        GameObject powerUp = new GameObject("PowerUpPrefab");
        powerUp.tag = "PowerUp";

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePowerUpSprite();
        sr.sortingOrder = 3;

        CircleCollider2D collider = powerUp.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.4f;

        Rigidbody2D rb = powerUp.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        powerUp.AddComponent<PowerUpController>();

        powerUp.SetActive(false);
        return powerUp;
    }

    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 5;
            mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.15f);
            mainCamera.transform.position = new Vector3(0, 0, -10);
        }
    }

    void SetupBackground()
    {
        if (FindObjectOfType<ParallaxBackground>() == null)
        {
            GameObject bgObj = new GameObject("ParallaxBackground");
            bgObj.AddComponent<ParallaxBackground>();
            Debug.Log("ParallaxBackground created");
        }
    }
}
