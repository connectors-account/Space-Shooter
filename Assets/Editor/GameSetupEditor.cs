using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Editor script to automatically set up the Space Shooter game scene.
/// Access via menu: Tools > Space Shooter > Setup Game Scene
/// </summary>
public class GameSetupEditor : EditorWindow
{
    [MenuItem("Tools/Space Shooter/Setup Game Scene")]
    public static void SetupGameScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Game Scene",
            "This will set up all game objects in the current scene. Continue?",
            "Yes", "Cancel"))
        {
            return;
        }
        
        CreateTags();
        CreateCamera();
        CreateGameManagers();
        CreateUI();
        CreateSpawner();
        CreateBackground();
        
        EditorUtility.DisplayDialog("Setup Complete",
            "Game scene setup is complete!\n\n" +
            "Press Play to test the game.\n\n" +
            "Controls:\n" +
            "- WASD/Arrow Keys: Move\n" +
            "- Space/Left Click: Shoot\n" +
            "- ESC/P: Pause",
            "OK");
    }
    
    [MenuItem("Tools/Space Shooter/Create Player Prefab")]
    public static void CreatePlayerPrefab()
    {
        // Create player object
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.transform.position = Vector3.zero;
        player.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        player.tag = "Player";
        
        // Remove 3D collider
        Object.DestroyImmediate(player.GetComponent<BoxCollider>());
        
        // Add 2D collider
        BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.6f, 0.8f);
        
        // Add Rigidbody2D
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Add player components
        player.AddComponent<PlayerController>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerShooting>();
        
        // Set material color
        MeshRenderer renderer = player.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.cyan;
            renderer.material = mat;
        }
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/Player.prefab";
        EnsureFolderExists("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
        Object.DestroyImmediate(player);
        
        Debug.Log($"Player prefab created at {prefabPath}");
    }
    
    [MenuItem("Tools/Space Shooter/Create Enemy Prefab")]
    public static void CreateEnemyPrefab()
    {
        // Create enemy object
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "Enemy";
        enemy.transform.position = Vector3.zero;
        enemy.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        enemy.tag = "Enemy";
        
        // Remove 3D collider
        Object.DestroyImmediate(enemy.GetComponent<BoxCollider>());
        
        // Add 2D collider
        BoxCollider2D collider = enemy.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.8f, 0.8f);
        
        // Add Rigidbody2D
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        // Add enemy components
        enemy.AddComponent<EnemyController>();
        enemy.AddComponent<EnemyHealth>();
        
        // Set material color
        MeshRenderer renderer = enemy.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.red;
            renderer.material = mat;
        }
        
        // Save as prefab
        string prefabPath = "Assets/Prefabs/Enemy.prefab";
        EnsureFolderExists("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);
        Object.DestroyImmediate(enemy);
        
        Debug.Log($"Enemy prefab created at {prefabPath}");
    }
    
    [MenuItem("Tools/Space Shooter/Create Bullet Prefab")]
    public static void CreateBulletPrefab()
    {
        // Create player bullet
        GameObject playerBullet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerBullet.name = "PlayerBullet";
        playerBullet.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
        playerBullet.tag = "PlayerBullet";
        
        Object.DestroyImmediate(playerBullet.GetComponent<BoxCollider>());
        BoxCollider2D col1 = playerBullet.AddComponent<BoxCollider2D>();
        col1.isTrigger = true;
        col1.size = new Vector2(0.2f, 0.4f);
        
        Rigidbody2D rb1 = playerBullet.AddComponent<Rigidbody2D>();
        rb1.gravityScale = 0f;
        
        playerBullet.AddComponent<Bullet>();
        
        MeshRenderer renderer1 = playerBullet.GetComponent<MeshRenderer>();
        if (renderer1 != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.cyan;
            renderer1.material = mat;
        }
        
        string prefabPath1 = "Assets/Prefabs/PlayerBullet.prefab";
        EnsureFolderExists("Assets/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(playerBullet, prefabPath1);
        Object.DestroyImmediate(playerBullet);
        
        // Create enemy bullet
        GameObject enemyBullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemyBullet.name = "EnemyBullet";
        enemyBullet.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        enemyBullet.tag = "EnemyBullet";
        
        Object.DestroyImmediate(enemyBullet.GetComponent<SphereCollider>());
        CircleCollider2D col2 = enemyBullet.AddComponent<CircleCollider2D>();
        col2.isTrigger = true;
        col2.radius = 0.125f;
        
        Rigidbody2D rb2 = enemyBullet.AddComponent<Rigidbody2D>();
        rb2.gravityScale = 0f;
        
        enemyBullet.AddComponent<Bullet>();
        
        MeshRenderer renderer2 = enemyBullet.GetComponent<MeshRenderer>();
        if (renderer2 != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            renderer2.material = mat;
        }
        
        string prefabPath2 = "Assets/Prefabs/EnemyBullet.prefab";
        PrefabUtility.SaveAsPrefabAsset(enemyBullet, prefabPath2);
        Object.DestroyImmediate(enemyBullet);
        
        Debug.Log("Bullet prefabs created!");
    }
    
    private static void CreateTags()
    {
        // Tags are defined in TagManager.asset
        // This ensures they're registered
        Debug.Log("Tags should be defined in ProjectSettings/TagManager.asset");
    }
    
    private static void CreateCamera()
    {
        // Find or create main camera
        Camera mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";
        }
        
        // Configure camera for 2D
        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.backgroundColor = new Color(0.05f, 0.05f, 0.15f);
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        
        Debug.Log("Camera configured for 2D gameplay");
    }
    
    private static void CreateGameManagers()
    {
        // Create GameManager
        if (Object.FindObjectOfType<GameManager>() == null)
        {
            GameObject gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();
            Debug.Log("GameManager created");
        }
        
        // Create ScoreManager
        if (Object.FindObjectOfType<ScoreManager>() == null)
        {
            GameObject scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.AddComponent<ScoreManager>();
            Debug.Log("ScoreManager created");
        }
    }
    
    private static void CreateUI()
    {
        // Create Canvas with UIManager
        if (Object.FindObjectOfType<UIManager>() == null)
        {
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            canvasObj.AddComponent<UIManager>();
            
            // Create EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            Debug.Log("UI Canvas and EventSystem created");
        }
    }
    
    private static void CreateSpawner()
    {
        // Create EnemySpawner
        if (Object.FindObjectOfType<EnemySpawner>() == null)
        {
            GameObject spawnerObj = new GameObject("EnemySpawner");
            spawnerObj.AddComponent<EnemySpawner>();
            Debug.Log("EnemySpawner created");
        }
    }
    
    private static void CreateBackground()
    {
        // Create a simple star background
        GameObject background = new GameObject("Background");
        background.transform.position = new Vector3(0f, 0f, 5f);
        
        // Add some decorative elements (optional)
        for (int i = 0; i < 50; i++)
        {
            GameObject star = GameObject.CreatePrimitive(PrimitiveType.Quad);
            star.name = "Star";
            star.transform.parent = background.transform;
            star.transform.localPosition = new Vector3(
                Random.Range(-10f, 10f),
                Random.Range(-6f, 6f),
                0f
            );
            float size = Random.Range(0.02f, 0.08f);
            star.transform.localScale = new Vector3(size, size, size);
            
            // Remove collider
            Object.DestroyImmediate(star.GetComponent<MeshCollider>());
            
            // Set star color
            MeshRenderer renderer = star.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = new Color(1f, 1f, 1f, Random.Range(0.3f, 1f));
                renderer.material = mat;
            }
        }
        
        Debug.Log("Background created with stars");
    }
    
    private static void EnsureFolderExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string[] folders = path.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
    }
}
