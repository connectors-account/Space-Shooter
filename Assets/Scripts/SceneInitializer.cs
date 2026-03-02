using UnityEngine;

/// <summary>
/// SceneInitializer sets up the entire game scene at runtime.
/// This is the main entry point that creates all necessary game objects.
/// Attach this to a single empty GameObject in your scene.
/// </summary>
public class SceneInitializer : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("Whether to create everything at runtime")]
    public bool createSceneAtRuntime = true;

    void Awake()
    {
        if (createSceneAtRuntime)
        {
            SetupScene();
        }
    }

    /// <summary>
    /// Create the entire game scene
    /// </summary>
    void SetupScene()
    {
        // 1. Setup Camera
        SetupCamera();
        
        // 2. Create Game Managers
        CreateGameManagers();
        
        // 3. Create Background
        CreateBackground();
        
        // 4. UI will be created by UISetup component
        GameObject uiSetup = new GameObject("UISetup");
        uiSetup.AddComponent<UISetup>();
        
        // 5. Game Setup (creates player, prefabs, etc.)
        GameObject gameSetup = new GameObject("GameSetup");
        gameSetup.AddComponent<GameSetup>();
        
        Debug.Log("Space Shooter scene initialized successfully!");
    }

    /// <summary>
    /// Setup the main camera
    /// </summary>
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCamera = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
        }
        
        // Set camera properties for 2D
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.15f); // Dark blue background
        mainCamera.transform.position = new Vector3(0, 0, -10);
        
        // Add screen shake component
        if (mainCamera.GetComponent<ScreenShake>() == null)
        {
            mainCamera.gameObject.AddComponent<ScreenShake>();
        }
    }

    /// <summary>
    /// Create all game manager objects
    /// </summary>
    void CreateGameManagers()
    {
        // Game Manager
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }
        
        // Score Manager
        if (ScoreManager.Instance == null)
        {
            GameObject smObj = new GameObject("ScoreManager");
            smObj.AddComponent<ScoreManager>();
        }
        
        // Enemy Spawner
        if (EnemySpawner.Instance == null)
        {
            GameObject esObj = new GameObject("EnemySpawner");
            esObj.AddComponent<EnemySpawner>();
        }
        
        // Power-Up Spawner
        if (FindObjectOfType<PowerUpSpawner>() == null)
        {
            GameObject psObj = new GameObject("PowerUpSpawner");
            psObj.AddComponent<PowerUpSpawner>();
        }
    }

    /// <summary>
    /// Create scrolling background
    /// </summary>
    void CreateBackground()
    {
        // Create a simple starfield background
        for (int i = 0; i < 2; i++)
        {
            GameObject bg = new GameObject("Background_" + i);
            bg.transform.position = new Vector3(0, i * 20f, 5f); // Behind other objects
            
            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = CreateStarfieldSprite();
            sr.sortingOrder = -100;
            
            ScrollingBackground scroll = bg.AddComponent<ScrollingBackground>();
            scroll.scrollSpeed = 1f;
            scroll.resetHeight = -20f;
            scroll.resetOffset = 40f;
        }
    }

    /// <summary>
    /// Create a simple starfield sprite procedurally
    /// </summary>
    Sprite CreateStarfieldSprite()
    {
        int width = 256;
        int height = 512;
        
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        // Fill with dark blue/black
        Color bgColor = new Color(0.02f, 0.02f, 0.08f, 1f);
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = bgColor;
        }
        
        // Add stars
        System.Random rand = new System.Random(42); // Fixed seed for consistency
        int starCount = 200;
        
        for (int i = 0; i < starCount; i++)
        {
            int x = rand.Next(width);
            int y = rand.Next(height);
            float brightness = (float)rand.NextDouble() * 0.5f + 0.5f;
            
            Color starColor = new Color(brightness, brightness, brightness * 1.1f, 1f);
            
            int index = y * width + x;
            if (index < pixels.Length)
            {
                pixels[index] = starColor;
            }
        }
        
        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        
        // Create sprite that covers the screen
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 25.6f);
    }
}
