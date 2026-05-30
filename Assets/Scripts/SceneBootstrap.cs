using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ONE-CLICK SCENE SETUP — Attach this to any GameObject and press Play.
/// It builds the entire scene hierarchy (camera, player, enemies, UI, background)
/// using procedural sprites so no external assets are needed.
///
/// After running once in the Editor you can convert runtime objects to prefabs,
/// tweak values, and remove this script if desired.
/// </summary>
public class SceneBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Only bootstrap if the essential managers are missing
        if (GameManager.Instance != null) return;

        BuildScene();
    }

    void BuildScene()
    {
        // ── Camera ──
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5f;
        Camera.main.backgroundColor = new Color(0.02f, 0.02f, 0.08f);

        // ── GameManager ──
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();

        // ── SoundManager ──
        GameObject smObj = new GameObject("SoundManager");
        smObj.AddComponent<SoundManager>();

        // ── Background ──
        CreateBackground();

        // ── Player ──
        GameObject player = CreatePlayer();

        // ── Bullet Template (inactive, used as prefab) ──
        GameObject bulletTemplate = CreateBulletPrefab();

        // ── PowerUp Template ──
        GameObject powerUpTemplate = CreatePowerUpPrefab();

        // ── Enemy Templates ──
        GameObject enemyBasic  = CreateEnemyPrefab("Enemy_Basic",  Enemy.MovePattern.Straight, Color.red,     30, 100, bulletTemplate, powerUpTemplate);
        GameObject enemyZigzag = CreateEnemyPrefab("Enemy_Zigzag", Enemy.MovePattern.Zigzag,   new Color(1f, 0.5f, 0f), 40, 150, bulletTemplate, powerUpTemplate);
        GameObject enemySine   = CreateEnemyPrefab("Enemy_Sine",   Enemy.MovePattern.Sine,     Color.magenta, 50, 200, bulletTemplate, powerUpTemplate);
        GameObject enemyDiver  = CreateEnemyPrefab("Enemy_Diver",  Enemy.MovePattern.Dive,     Color.yellow,  25, 250, bulletTemplate, powerUpTemplate);

        // Wire player bullet prefab
        var pc = player.GetComponent<PlayerController>();
        pc.bulletPrefab = bulletTemplate;

        // ── Enemy Spawner ──
        GameObject spawner = new GameObject("EnemySpawner");
        var es = spawner.AddComponent<EnemySpawner>();
        es.enemyPrefabs = new GameObject[] { enemyBasic, enemyZigzag, enemySine, enemyDiver };

        // ── UI Canvas ──
        CreateUI();

        // ── Event System (required for UI buttons) ──
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSys = new GameObject("EventSystem");
            eventSys.AddComponent<EventSystem>();
            eventSys.AddComponent<StandaloneInputModule>();
        }
    }

    // ────────────────────────────────────────────────────────
    //  Factory helpers
    // ────────────────────────────────────────────────────────

    GameObject CreatePlayer()
    {
        GameObject go = new GameObject("Player");
        go.tag = "Player";
        go.transform.position = new Vector3(0, -3.5f, 0);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteGenerator.CreatePlayerShip();
        sr.sortingOrder = 2;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        go.AddComponent<PlayerController>();

        // Fire point
        GameObject fp = new GameObject("FirePoint");
        fp.transform.SetParent(go.transform);
        fp.transform.localPosition = new Vector3(0, 0.6f, 0);
        go.GetComponent<PlayerController>().firePoint = fp.transform;

        return go;
    }

    GameObject CreateBulletPrefab()
    {
        GameObject go = new GameObject("BulletPrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteGenerator.CreateBullet();
        sr.sortingOrder = 3;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.2f, 0.2f);

        go.AddComponent<Bullet>();
        go.SetActive(false); // template — copies are instantiated
        return go;
    }

    GameObject CreatePowerUpPrefab()
    {
        GameObject go = new GameObject("PowerUpPrefab");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteGenerator.CreatePowerUp();
        sr.sortingOrder = 3;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        go.tag = "PowerUp";
        go.AddComponent<PowerUp>();
        go.SetActive(false); // template
        return go;
    }

    GameObject CreateEnemyPrefab(string name, Enemy.MovePattern pattern, Color color, int hp, int score, GameObject bulletPrefab, GameObject powerUpPrefab)
    {
        GameObject go = new GameObject(name);
        go.tag = "Enemy";

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ProceduralSpriteGenerator.CreateEnemyShip(32, color);
        sr.sortingOrder = 2;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.7f, 0.7f);

        var enemy = go.AddComponent<Enemy>();
        enemy.pattern = pattern;
        enemy.health = hp;
        enemy.scoreValue = score;
        enemy.bulletPrefab = bulletPrefab;
        enemy.powerUpPrefab = powerUpPrefab;

        go.SetActive(false); // template
        return go;
    }

    void CreateBackground()
    {
        // Layer 1 — far stars (slow)
        GameObject bg1 = new GameObject("BG_Layer1");
        var sr1 = bg1.AddComponent<SpriteRenderer>();
        sr1.sprite = ProceduralSpriteGenerator.CreateStarfield(256, 512, 80);
        sr1.sortingOrder = -10;
        bg1.transform.position = new Vector3(0, 0, 5);
        bg1.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        var bs1 = bg1.AddComponent<BackgroundScroller>();
        bs1.scrollSpeed = 0.5f;

        // Layer 2 — near stars (faster)
        GameObject bg2 = new GameObject("BG_Layer2");
        var sr2 = bg2.AddComponent<SpriteRenderer>();
        sr2.sprite = ProceduralSpriteGenerator.CreateStarfield(256, 512, 40);
        sr2.sortingOrder = -9;
        bg2.transform.position = new Vector3(0, 0, 4);
        bg2.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        var bs2 = bg2.AddComponent<BackgroundScroller>();
        bs2.scrollSpeed = 1.2f;
    }

    void CreateUI()
    {
        GameObject canvas = new GameObject("UICanvas");
        canvas.AddComponent<UIManager>();
        // UIManager.BuildUI() will handle the rest if panels aren't assigned
    }
}
