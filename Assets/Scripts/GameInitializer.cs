using UnityEngine;

/// <summary>
/// Initializes the game by creating all necessary game objects and components.
/// Attach this to a single GameObject in the scene to bootstrap the entire game.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Initialize Settings")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool createUI = true;
    [SerializeField] private bool createPrefabs = true;
    
    private void Start()
    {
        if (initializeOnStart)
        {
            InitializeGame();
        }
    }
    
    /// <summary>
    /// Initialize all game systems
    /// </summary>
    public void InitializeGame()
    {
        Debug.Log("Initializing Space Shooter Game...");
        
        // Create managers
        CreateManagers();
        
        // Create prefabs
        if (createPrefabs)
        {
            CreatePrefabs();
        }
        
        // Create UI
        if (createUI)
        {
            CreateUI();
        }
        
        // Create background
        CreateBackground();
        
        // Setup camera
        SetupCamera();
        
        Debug.Log("Game initialization complete!");
    }
    
    private void CreateManagers()
    {
        // Game Manager
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            GameManager gm = gmObj.AddComponent<GameManager>();
        }
        
        // Wave Manager
        if (WaveManager.Instance == null)
        {
            GameObject wmObj = new GameObject("WaveManager");
            wmObj.AddComponent<WaveManager>();
        }
        
        // Audio Manager
        if (AudioManager.Instance == null)
        {
            GameObject amObj = new GameObject("AudioManager");
            amObj.AddComponent<AudioManager>();
        }
        
        // Collision Handler
        if (CollisionHandler.Instance == null)
        {
            GameObject chObj = new GameObject("CollisionHandler");
            chObj.AddComponent<CollisionHandler>();
        }
    }
    
    private void CreatePrefabs()
    {
        // Create prefab container
        GameObject prefabContainer = new GameObject("Prefabs");
        prefabContainer.SetActive(false);
        
        // Create Player Prefab
        GameObject playerPrefab = CreatePlayerPrefab();
        playerPrefab.transform.SetParent(prefabContainer.transform);
        
        // Create Enemy Prefabs
        GameObject basicEnemy = CreateEnemyPrefab("BasicEnemy", EnemyController.EnemyType.Basic);
        basicEnemy.transform.SetParent(prefabContainer.transform);
        
        GameObject zigzagEnemy = CreateEnemyPrefab("ZigzagEnemy", EnemyController.EnemyType.Zigzag);
        zigzagEnemy.transform.SetParent(prefabContainer.transform);
        
        GameObject shooterEnemy = CreateEnemyPrefab("ShooterEnemy", EnemyController.EnemyType.Shooter);
        shooterEnemy.transform.SetParent(prefabContainer.transform);
        
        // Create Bullet Prefabs
        GameObject playerBullet = CreateBulletPrefab("PlayerBullet", true);
        playerBullet.transform.SetParent(prefabContainer.transform);
        
        GameObject enemyBullet = CreateBulletPrefab("EnemyBullet", false);
        enemyBullet.transform.SetParent(prefabContainer.transform);
        
        // Create Power-up Prefabs
        GameObject shieldPowerUp = CreatePowerUpPrefab("ShieldPowerUp", PowerUpController.PowerUpType.Shield);
        shieldPowerUp.transform.SetParent(prefabContainer.transform);
        
        GameObject rapidFirePowerUp = CreatePowerUpPrefab("RapidFirePowerUp", PowerUpController.PowerUpType.RapidFire);
        rapidFirePowerUp.transform.SetParent(prefabContainer.transform);
        
        GameObject healthPowerUp = CreatePowerUpPrefab("HealthPowerUp", PowerUpController.PowerUpType.Health);
        healthPowerUp.transform.SetParent(prefabContainer.transform);
        
        // Create Explosion Prefab
        GameObject explosionPrefab = CreateExplosionPrefab();
        explosionPrefab.transform.SetParent(prefabContainer.transform);
        
        // Assign prefabs to managers
        AssignPrefabsToManagers(playerPrefab, basicEnemy, zigzagEnemy, shooterEnemy, 
                                 playerBullet, enemyBullet, shieldPowerUp, rapidFirePowerUp, 
                                 healthPowerUp, explosionPrefab);
    }
    
    private GameObject CreatePlayerPrefab()
    {
        GameObject player = new GameObject("Player");
        
        // Sprite Renderer
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePlayerShip();
        sr.sortingOrder = 5;
        
        // Components
        player.AddComponent<PlayerController>();
        
        HealthSystem health = player.AddComponent<HealthSystem>();
        // Health settings configured via serialized fields in editor
        
        // Collider
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.8f, 0.8f);
        collider.isTrigger = true;
        
        // Rigidbody
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        
        // Fire point
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(player.transform);
        firePoint.transform.localPosition = new Vector3(0, 0.6f, 0);
        
        player.tag = "Player";
        
        return player;
    }
    
    private GameObject CreateEnemyPrefab(string name, EnemyController.EnemyType type)
    {
        GameObject enemy = new GameObject(name);
        
        // Sprite Renderer
        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateEnemyShip(64, (int)type);
        sr.sortingOrder = 4;
        
        // Components
        EnemyController controller = enemy.AddComponent<EnemyController>();
        enemy.AddComponent<HealthSystem>();
        
        // Collider
        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.7f, 0.7f);
        collider.isTrigger = true;
        
        // Rigidbody
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        
        enemy.tag = "Enemy";
        
        return enemy;
    }
    
    private GameObject CreateBulletPrefab(string name, bool isPlayerBullet)
    {
        GameObject bullet = new GameObject(name);
        
        // Sprite Renderer
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateBullet(16, isPlayerBullet);
        sr.sortingOrder = 3;
        
        // Components
        BulletController controller = bullet.AddComponent<BulletController>();
        
        // Collider
        CircleCollider2D collider = bullet.AddComponent<CircleCollider2D>();
        collider.radius = 0.15f;
        collider.isTrigger = true;
        
        // Rigidbody
        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        
        bullet.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        
        return bullet;
    }
    
    private GameObject CreatePowerUpPrefab(string name, PowerUpController.PowerUpType type)
    {
        GameObject powerUp = new GameObject(name);
        
        // Sprite Renderer
        SpriteRenderer sr = powerUp.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreatePowerUp(32, (int)type);
        sr.sortingOrder = 4;
        
        // Components
        PowerUpController controller = powerUp.AddComponent<PowerUpController>();
        controller.Initialize(type);
        
        // Collider
        CircleCollider2D collider = powerUp.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;
        collider.isTrigger = true;
        
        powerUp.tag = "PowerUp";
        
        return powerUp;
    }
    
    private GameObject CreateExplosionPrefab()
    {
        GameObject explosion = new GameObject("Explosion");
        
        // Sprite Renderer
        SpriteRenderer sr = explosion.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteGenerator.CreateExplosion();
        sr.sortingOrder = 10;
        
        // Add simple animation script
        explosion.AddComponent<ExplosionEffect>();
        
        return explosion;
    }
    
    private void AssignPrefabsToManagers(GameObject player, GameObject basicEnemy, 
        GameObject zigzagEnemy, GameObject shooterEnemy, GameObject playerBullet,
        GameObject enemyBullet, GameObject shieldPowerUp, GameObject rapidFirePowerUp,
        GameObject healthPowerUp, GameObject explosion)
    {
        // Assign to WaveManager
        WaveManager wm = WaveManager.Instance;
        if (wm != null)
        {
            // Use reflection or SerializedObject in editor to assign
            // For runtime, we need to use a different approach
        }
        
        // Store prefabs for runtime access
        PrefabManager.Instance?.RegisterPrefabs(player, basicEnemy, zigzagEnemy, shooterEnemy,
            playerBullet, enemyBullet, shieldPowerUp, rapidFirePowerUp, healthPowerUp, explosion);
    }
    
    private void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Create EventSystem if not exists
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Add UI Manager
        UIManager uiManager = canvasObj.AddComponent<UIManager>();
        uiManager.CreateUIElements();
        
        // Add Menu Manager
        MenuManager menuManager = canvasObj.AddComponent<MenuManager>();
        menuManager.CreateMenuUI();
    }
    
    private void CreateBackground()
    {
        GameObject background = new GameObject("ParallaxBackground");
        background.AddComponent<ParallaxBackground>();
    }
    
    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        }
    }
}

/// <summary>
/// Simple explosion effect component
/// </summary>
public class ExplosionEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float lifetime = 0.5f;
    private float elapsed = 0f;
    private Vector3 startScale;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
    }
    
    private void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / lifetime;
        
        // Scale up
        transform.localScale = startScale * (1f + progress * 0.5f);
        
        // Fade out
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f - progress;
            spriteRenderer.color = c;
        }
        
        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Singleton prefab manager for runtime prefab access
/// </summary>
public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance { get; private set; }
    
    public GameObject PlayerPrefab { get; private set; }
    public GameObject BasicEnemyPrefab { get; private set; }
    public GameObject ZigzagEnemyPrefab { get; private set; }
    public GameObject ShooterEnemyPrefab { get; private set; }
    public GameObject PlayerBulletPrefab { get; private set; }
    public GameObject EnemyBulletPrefab { get; private set; }
    public GameObject ShieldPowerUpPrefab { get; private set; }
    public GameObject RapidFirePowerUpPrefab { get; private set; }
    public GameObject HealthPowerUpPrefab { get; private set; }
    public GameObject ExplosionPrefab { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public void RegisterPrefabs(GameObject player, GameObject basicEnemy, GameObject zigzagEnemy,
        GameObject shooterEnemy, GameObject playerBullet, GameObject enemyBullet,
        GameObject shieldPowerUp, GameObject rapidFirePowerUp, GameObject healthPowerUp, GameObject explosion)
    {
        PlayerPrefab = player;
        BasicEnemyPrefab = basicEnemy;
        ZigzagEnemyPrefab = zigzagEnemy;
        ShooterEnemyPrefab = shooterEnemy;
        PlayerBulletPrefab = playerBullet;
        EnemyBulletPrefab = enemyBullet;
        ShieldPowerUpPrefab = shieldPowerUp;
        RapidFirePowerUpPrefab = rapidFirePowerUp;
        HealthPowerUpPrefab = healthPowerUp;
        ExplosionPrefab = explosion;
    }
}
