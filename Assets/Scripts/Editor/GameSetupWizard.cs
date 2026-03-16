#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// GameSetupWizard helps setup the game scene with all required objects.
/// Run from Unity Editor: Tools > Space Shooter > Setup Game Scene
/// </summary>
public class GameSetupWizard : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Setup Game Scene")]
    public static void ShowWindow()
    {
        GetWindow<GameSetupWizard>("Game Setup Wizard");
    }

    private void OnGUI()
    {
        GUILayout.Label("Space Shooter Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "This wizard will help you set up the game scene with all required objects.\n" +
            "Make sure you have generated sprites first using Tools > Space Shooter > Generate Sprites",
            MessageType.Info);

        GUILayout.Space(20);

        if (GUILayout.Button("1. Create Game Managers", GUILayout.Height(30)))
        {
            CreateManagers();
        }

        if (GUILayout.Button("2. Create Player", GUILayout.Height(30)))
        {
            CreatePlayer();
        }

        if (GUILayout.Button("3. Create Enemy Prefabs", GUILayout.Height(30)))
        {
            CreateEnemyPrefabs();
        }

        if (GUILayout.Button("4. Create Bullet Prefabs", GUILayout.Height(30)))
        {
            CreateBulletPrefabs();
        }

        if (GUILayout.Button("5. Create PowerUp Prefabs", GUILayout.Height(30)))
        {
            CreatePowerUpPrefabs();
        }

        if (GUILayout.Button("6. Create UI Canvas", GUILayout.Height(30)))
        {
            CreateUICanvas();
        }

        if (GUILayout.Button("7. Create Background", GUILayout.Height(30)))
        {
            CreateBackground();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Setup Everything (All Steps)", GUILayout.Height(40)))
        {
            SetupAll();
        }
    }

    private void SetupAll()
    {
        CreateManagers();
        CreatePlayer();
        CreateEnemyPrefabs();
        CreateBulletPrefabs();
        CreatePowerUpPrefabs();
        CreateUICanvas();
        CreateBackground();
        Debug.Log("Game scene setup complete!");
    }

    private void CreateManagers()
    {
        // Game Manager
        if (FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        // Score Manager
        if (FindFirstObjectByType<ScoreManager>() == null)
        {
            GameObject smObj = new GameObject("ScoreManager");
            smObj.AddComponent<ScoreManager>();
        }

        // Wave Manager
        if (FindFirstObjectByType<WaveManager>() == null)
        {
            GameObject wmObj = new GameObject("WaveManager");
            wmObj.AddComponent<WaveManager>();
        }

        // Enemy Spawner
        if (FindFirstObjectByType<EnemySpawner>() == null)
        {
            GameObject esObj = new GameObject("EnemySpawner");
            esObj.AddComponent<EnemySpawner>();
        }

        // PowerUp Spawner
        if (FindFirstObjectByType<PowerUpSpawner>() == null)
        {
            GameObject psObj = new GameObject("PowerUpSpawner");
            psObj.AddComponent<PowerUpSpawner>();
        }

        // Sound Manager
        if (FindFirstObjectByType<SoundManager>() == null)
        {
            GameObject sndObj = new GameObject("SoundManager");
            sndObj.AddComponent<SoundManager>();
        }

        // Object Pooler
        if (FindFirstObjectByType<ObjectPooler>() == null)
        {
            GameObject opObj = new GameObject("ObjectPooler");
            opObj.AddComponent<ObjectPooler>();
        }

        Debug.Log("Game managers created!");
    }

    private void CreatePlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            Debug.Log("Player already exists!");
            return;
        }

        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3f, 0);

        // Add sprite renderer
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Player";
        sr.sortingOrder = 10;

        // Add collider
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.8f);

        // Add rigidbody
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();

        // Create shield child
        GameObject shield = new GameObject("Shield");
        shield.transform.SetParent(player.transform);
        shield.transform.localPosition = Vector3.zero;
        shield.transform.localScale = Vector3.one * 1.5f;
        SpriteRenderer shieldSr = shield.AddComponent<SpriteRenderer>();
        shieldSr.sortingOrder = 11;
        shield.SetActive(false);

        // Create prefab
        SaveAsPrefab(player, "Assets/Prefabs/Player/Player.prefab");

        Debug.Log("Player created!");
    }

    private void CreateEnemyPrefabs()
    {
        CreateEnemyPrefab("SmallEnemy", typeof(SmallEnemy), 0.5f, "enemy_small");
        CreateEnemyPrefab("MediumEnemy", typeof(MediumEnemy), 0.75f, "enemy_medium");
        CreateEnemyPrefab("LargeEnemy", typeof(LargeEnemy), 1f, "enemy_large");
        CreateEnemyPrefab("TrackerEnemy", typeof(TrackerEnemy), 0.75f, "enemy_tracker");
        CreateEnemyPrefab("BossEnemy", typeof(BossEnemy), 2f, "enemy_boss");

        // Create explosion prefab
        GameObject explosion = new GameObject("Explosion");
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.color = Color.yellow;
        explosion.AddComponent<Explosion>();
        SaveAsPrefab(explosion, "Assets/Prefabs/Effects/Explosion.prefab");
        DestroyImmediate(explosion);

        Debug.Log("Enemy prefabs created!");
    }

    private void CreateEnemyPrefab(string name, System.Type enemyType, float scale, string spriteName)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";
        enemy.transform.localScale = Vector3.one * scale;

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Enemies";
        sr.sortingOrder = 5;

        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        enemy.AddComponent(enemyType);

        SaveAsPrefab(enemy, $"Assets/Prefabs/Enemies/{name}.prefab");
        DestroyImmediate(enemy);
    }

    private void CreateBulletPrefabs()
    {
        // Player bullet
        CreateBulletPrefab("PlayerBullet", "PlayerBullet", Color.cyan, new Vector2(0.15f, 0.4f));

        // Enemy bullet
        CreateBulletPrefab("EnemyBullet", "EnemyBullet", Color.red, new Vector2(0.2f, 0.2f));

        Debug.Log("Bullet prefabs created!");
    }

    private void CreateBulletPrefab(string name, string tag, Color color, Vector2 size)
    {
        GameObject bullet = new GameObject(name);
        bullet.tag = tag;

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingLayerName = "Bullets";
        sr.sortingOrder = 8;

        BoxCollider2D collider = bullet.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = size;

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        Bullet bulletScript = bullet.AddComponent<Bullet>();

        SaveAsPrefab(bullet, $"Assets/Prefabs/Bullets/{name}.prefab");
        DestroyImmediate(bullet);
    }

    private void CreatePowerUpPrefabs()
    {
        CreatePowerUpPrefab("PowerUp_Weapon", PowerUp.PowerUpType.WeaponUpgrade, Color.yellow);
        CreatePowerUpPrefab("PowerUp_Shield", PowerUp.PowerUpType.Shield, Color.cyan);
        CreatePowerUpPrefab("PowerUp_Health", PowerUp.PowerUpType.Health, Color.green);
        CreatePowerUpPrefab("PowerUp_Score", PowerUp.PowerUpType.ScoreBonus, new Color(1f, 0.5f, 0f));

        Debug.Log("PowerUp prefabs created!");
    }

    private void CreatePowerUpPrefab(string name, PowerUp.PowerUpType type, Color color)
    {
        GameObject powerUp = new GameObject(name);
        powerUp.tag = "PowerUp";

        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.color = color;
        sr.sortingLayerName = "PowerUps";
        sr.sortingOrder = 6;

        CircleCollider2D collider = powerUp.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.4f;

        PowerUp puScript = powerUp.AddComponent<PowerUp>();
        // Note: SetType would need to be called after instantiation

        SaveAsPrefab(powerUp, $"Assets/Prefabs/PowerUps/{name}.prefab");
        DestroyImmediate(powerUp);
    }

    private void CreateUICanvas()
    {
        if (FindFirstObjectByType<Canvas>() != null)
        {
            Debug.Log("Canvas already exists!");
            return;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Add UIManager
        canvasObj.AddComponent<UIManager>();

        // Create HUD Panel
        GameObject hudPanel = CreatePanel(canvasObj.transform, "HUDPanel");
        
        // Create Main Menu Panel
        GameObject mainMenuPanel = CreatePanel(canvasObj.transform, "MainMenuPanel");
        mainMenuPanel.AddComponent<MainMenuUI>();
        
        // Create Pause Panel
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PauseMenuPanel");
        pausePanel.AddComponent<PauseMenuUI>();
        pausePanel.SetActive(false);
        
        // Create Game Over Panel
        GameObject gameOverPanel = CreatePanel(canvasObj.transform, "GameOverPanel");
        gameOverPanel.AddComponent<GameOverUI>();
        gameOverPanel.SetActive(false);

        Debug.Log("UI Canvas created! Add TextMeshPro elements manually.");
    }

    private GameObject CreatePanel(Transform parent, string name)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return panel;
    }

    private void CreateBackground()
    {
        if (GameObject.Find("Background") != null)
        {
            Debug.Log("Background already exists!");
            return;
        }

        GameObject bg = new GameObject("Background");
        bg.transform.position = new Vector3(0, 0, 10);
        
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Background";
        sr.sortingOrder = -100;
        sr.color = new Color(0.02f, 0.02f, 0.08f);

        bg.AddComponent<InfiniteBackground>();

        Debug.Log("Background created!");
    }

    private void SaveAsPrefab(GameObject obj, string path)
    {
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        PrefabUtility.SaveAsPrefabAsset(obj, path);
    }
}
#endif
