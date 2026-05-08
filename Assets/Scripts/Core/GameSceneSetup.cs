using UnityEngine;

/// <summary>
/// Bootstraps the GameScene at runtime: creates all game objects, prefabs, pools,
/// and wires everything together. Attach this to an empty GameObject in the scene.
/// This is the ONLY script you need to manually add to the scene.
/// </summary>
public class GameSceneSetup : MonoBehaviour
{
    private void Awake()
    {
        SetupCamera();
        SetupManagers();
        SetupPrefabsAndPools();
        SetupPlayer();
        SetupBackground();
        SetupUI();
        StartMusic();
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.backgroundColor = new Color(0.01f, 0.01f, 0.05f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.transform.position = new Vector3(0, 0, -10);
    }

    private void SetupManagers()
    {
        // GameManager (singleton, persists)
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        // AudioManager (singleton, persists)
        if (AudioManager.Instance == null)
        {
            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
        }

        // ObjectPool (scene-level)
        if (ObjectPool.Instance == null)
        {
            GameObject poolObj = new GameObject("ObjectPool");
            poolObj.AddComponent<ObjectPool>();
        }

        // WaveManager
        if (WaveManager.Instance == null)
        {
            GameObject waveObj = new GameObject("WaveManager");
            waveObj.AddComponent<WaveManager>();
        }

        // PowerUpSpawner
        if (PowerUpSpawner.Instance == null)
        {
            GameObject puObj = new GameObject("PowerUpSpawner");
            puObj.AddComponent<PowerUpSpawner>();
        }
    }

    private void SetupPrefabsAndPools()
    {
        ObjectPool pool = ObjectPool.Instance;
        if (pool == null) return;

        // --- Player Bullet Prefab ---
        GameObject playerBulletPrefab = CreatePrefab("PlayerBulletPrefab");
        SpriteRenderer pbSr = playerBulletPrefab.AddComponent<SpriteRenderer>();
        pbSr.sprite = SpriteFactory.CreateBullet(6, 16, new Color(0.3f, 1f, 0.5f));
        pbSr.sortingOrder = 5;
        BoxCollider2D pbCol = playerBulletPrefab.AddComponent<BoxCollider2D>();
        pbCol.isTrigger = true;
        pbCol.size = new Vector2(0.3f, 0.6f);
        Rigidbody2D pbRb = playerBulletPrefab.AddComponent<Rigidbody2D>();
        pbRb.gravityScale = 0;
        pbRb.freezeRotation = true;
        playerBulletPrefab.AddComponent<Bullet>();
        PooledObject pbPo = playerBulletPrefab.AddComponent<PooledObject>();
        pbPo.poolTag = Tags.PlayerBullet;
        playerBulletPrefab.SetActive(false);
        pool.RegisterPool(Tags.PlayerBullet, playerBulletPrefab, 30);

        // --- Enemy Bullet Prefab ---
        GameObject enemyBulletPrefab = CreatePrefab("EnemyBulletPrefab");
        SpriteRenderer ebSr = enemyBulletPrefab.AddComponent<SpriteRenderer>();
        ebSr.sprite = SpriteFactory.CreateBullet(6, 12, new Color(1f, 0.3f, 0.2f));
        ebSr.sortingOrder = 5;
        BoxCollider2D ebCol = enemyBulletPrefab.AddComponent<BoxCollider2D>();
        ebCol.isTrigger = true;
        ebCol.size = new Vector2(0.3f, 0.5f);
        Rigidbody2D ebRb = enemyBulletPrefab.AddComponent<Rigidbody2D>();
        ebRb.gravityScale = 0;
        ebRb.freezeRotation = true;
        enemyBulletPrefab.AddComponent<Bullet>();
        PooledObject ebPo = enemyBulletPrefab.AddComponent<PooledObject>();
        ebPo.poolTag = Tags.EnemyBullet;
        enemyBulletPrefab.SetActive(false);
        pool.RegisterPool(Tags.EnemyBullet, enemyBulletPrefab, 30);

        // --- Enemy Basic Prefab ---
        CreateEnemyPrefab(Tags.EnemyBasic, new Color(1f, 0.3f, 0.3f), 32, 10);

        // --- Enemy Fast Prefab ---
        CreateEnemyPrefab(Tags.EnemyFast, new Color(1f, 0.8f, 0.2f), 24, 8);

        // --- Enemy Tank Prefab ---
        CreateEnemyPrefab(Tags.EnemyTank, new Color(0.5f, 0.5f, 0.8f), 40, 8);

        // --- Boss Prefab ---
        GameObject bossPrefab = CreatePrefab("BossPrefab");
        SpriteRenderer bossSr = bossPrefab.AddComponent<SpriteRenderer>();
        bossSr.sprite = SpriteFactory.CreateBossShip(64);
        bossSr.sortingOrder = 3;
        BoxCollider2D bossCol = bossPrefab.AddComponent<BoxCollider2D>();
        bossCol.isTrigger = true;
        bossCol.size = new Vector2(1.5f, 1.2f);
        Rigidbody2D bossRb = bossPrefab.AddComponent<Rigidbody2D>();
        bossRb.gravityScale = 0;
        bossRb.freezeRotation = true;
        bossPrefab.tag = Tags.Enemy;
        EnemyBase bossEnemy = bossPrefab.AddComponent<EnemyBase>();
        bossEnemy.SetPoolTag(Tags.EnemyBoss);
        PooledObject bossPo = bossPrefab.AddComponent<PooledObject>();
        bossPo.poolTag = Tags.EnemyBoss;
        bossPrefab.SetActive(false);
        pool.RegisterPool(Tags.EnemyBoss, bossPrefab, 3);

        // --- Explosion Prefab ---
        GameObject explosionPrefab = CreatePrefab("ExplosionPrefab");
        SpriteRenderer expSr = explosionPrefab.AddComponent<SpriteRenderer>();
        expSr.sprite = SpriteFactory.CreateCircle(16, new Color(1f, 0.6f, 0f));
        expSr.sortingOrder = 10;
        explosionPrefab.AddComponent<Explosion>();
        PooledObject expPo = explosionPrefab.AddComponent<PooledObject>();
        expPo.poolTag = Tags.Explosion;
        explosionPrefab.SetActive(false);
        pool.RegisterPool(Tags.Explosion, explosionPrefab, 20);

        // --- PowerUp Prefab ---
        GameObject powerUpPrefab = CreatePrefab("PowerUpPrefab");
        SpriteRenderer puSr = powerUpPrefab.AddComponent<SpriteRenderer>();
        puSr.sprite = SpriteFactory.CreatePowerUpGem(20);
        puSr.sortingOrder = 6;
        CircleCollider2D puCol = powerUpPrefab.AddComponent<CircleCollider2D>();
        puCol.isTrigger = true;
        puCol.radius = 0.4f;
        powerUpPrefab.AddComponent<PowerUp>();
        PooledObject puPooled = powerUpPrefab.AddComponent<PooledObject>();
        puPooled.poolTag = Tags.PowerUp;
        powerUpPrefab.SetActive(false);
        pool.RegisterPool(Tags.PowerUp, powerUpPrefab, 10);
    }

    private void CreateEnemyPrefab(string tag, Color color, int spriteSize, int poolSize)
    {
        ObjectPool pool = ObjectPool.Instance;
        GameObject prefab = CreatePrefab(tag + "Prefab");

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateEnemyShip(spriteSize, color);
        sr.sortingOrder = 3;

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.8f, 0.8f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        prefab.tag = Tags.Enemy;

        EnemyBase enemy = prefab.AddComponent<EnemyBase>();
        enemy.SetPoolTag(tag);

        PooledObject po = prefab.AddComponent<PooledObject>();
        po.poolTag = tag;

        prefab.SetActive(false);
        pool.RegisterPool(tag, prefab, poolSize);
    }

    private void SetupPlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = Tags.Player;
        player.transform.position = new Vector3(0, -3f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreatePlayerShip(48);
        sr.sortingOrder = 4;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        player.AddComponent<PlayerController>();
        PlayerHealth health = player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerShooting>();

        // Apply mouse control setting
        bool mouseControl = PlayerPrefs.GetInt("MouseControl", 0) == 1;
        player.GetComponent<PlayerController>().SetMouseControl(mouseControl);

        // Create shield visual (child object)
        GameObject shield = new GameObject("Shield");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
        shieldSr.sprite = SpriteFactory.CreateShieldRing(48);
        shieldSr.sortingOrder = 7;
        shield.SetActive(false);

        health.SetShieldVisual(shield);
    }

    private void SetupBackground()
    {
        GameObject bgObj = new GameObject("ParallaxBackground");
        bgObj.AddComponent<ParallaxBackground>();
    }

    private void SetupUI()
    {
        GameObject hudObj = new GameObject("HUD");
        hudObj.AddComponent<GameHUD>();

        GameObject gameOverObj = new GameObject("GameOverUI");
        gameOverObj.AddComponent<GameOverUI>();

        GameObject pauseObj = new GameObject("PauseMenuUI");
        pauseObj.AddComponent<PauseMenuUI>();

        GameObject waveObj = new GameObject("WaveAnnouncement");
        waveObj.AddComponent<WaveAnnouncement>();
    }

    private void StartMusic()
    {
        // Slight delay to let AudioManager initialize
        Invoke(nameof(PlayBGM), 0.5f);
    }

    private void PlayBGM()
    {
        AudioManager.Instance?.PlayMusic();
    }

    private GameObject CreatePrefab(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(ObjectPool.Instance.transform);
        return obj;
    }
}
