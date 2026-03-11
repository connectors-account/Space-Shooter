using UnityEngine;

/// <summary>
/// Initializes the game scene with all required components.
/// Attach this script to an empty GameObject in the scene, or it will auto-create everything.
/// This is a runtime initializer - use GameSetupEditor in the Editor for design-time setup.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    [Header("Initialization Settings")]
    [Tooltip("Auto-initialize if managers are missing")]
    [SerializeField] private bool autoInitialize = true;
    
    /// <summary>
    /// Initialize on Awake to ensure setup happens before Start of other scripts.
    /// </summary>
    private void Awake()
    {
        if (autoInitialize)
        {
            InitializeGame();
        }
    }
    
    /// <summary>
    /// Set up all required game systems.
    /// </summary>
    public void InitializeGame()
    {
        SetupTags();
        SetupCamera();
        SetupManagers();
        SetupUI();
        SetupSpawner();
        
        Debug.Log("Game initialization complete!");
    }
    
    /// <summary>
    /// Ensure required tags exist (Unity limitation - can't add tags at runtime).
    /// </summary>
    private void SetupTags()
    {
        // Tags must be defined in the Editor or via TagManager.asset
        // This method logs a warning if expected tags might be missing
        Debug.Log("Ensure tags are defined: Player, Enemy, PlayerBullet, EnemyBullet, Powerup");
    }
    
    /// <summary>
    /// Configure the main camera for 2D gameplay.
    /// </summary>
    private void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            // Create camera
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";
        }
        
        // Configure for 2D
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.15f, 1f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
    }
    
    /// <summary>
    /// Create game manager singletons if they don't exist.
    /// </summary>
    private void SetupManagers()
    {
        // GameManager
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
        
        // ScoreManager
        if (ScoreManager.Instance == null)
        {
            GameObject smObj = new GameObject("ScoreManager");
            smObj.AddComponent<ScoreManager>();
        }
    }
    
    /// <summary>
    /// Create UI canvas and manager if they don't exist.
    /// </summary>
    private void SetupUI()
    {
        if (UIManager.Instance == null)
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("GameCanvas");
            UnityEngine.Canvas canvas = canvasObj.AddComponent<UnityEngine.Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            canvasObj.AddComponent<UIManager>();
        }
        
        // Create EventSystem if needed
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventObj = new GameObject("EventSystem");
            eventObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }
    
    /// <summary>
    /// Create enemy spawner if it doesn't exist.
    /// </summary>
    private void SetupSpawner()
    {
        if (EnemySpawner.Instance == null)
        {
            GameObject spawnerObj = new GameObject("EnemySpawner");
            spawnerObj.AddComponent<EnemySpawner>();
        }
    }
}
