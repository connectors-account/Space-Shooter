using UnityEngine;

/// <summary>
/// Master setup script that bootstraps the entire game scene at runtime.
/// Attach this to a single empty GameObject named "GameSetup" in the scene.
/// It creates the Player, Managers, Background, and configures tags/physics.
/// This eliminates the need for pre-built prefabs and complex scene setup.
/// </summary>
public class GameSetup : MonoBehaviour
{
    private void Awake()
    {
        SetupTags();
        SetupPhysics();
        CreateGameManager();
        CreateUIManager();
        CreateAudioManager();
        CreatePlayer();
        CreateEnemySpawner();
        CreateParallaxBackground();
    }

    /// <summary>
    /// Configures the Physics2D layer collision matrix.
    /// </summary>
    private void SetupPhysics()
    {
        // Ensure 2D physics has reasonable settings
        Physics2D.gravity = Vector2.zero;
    }

    /// <summary>
    /// Ensures required tags exist. Tags must be defined in the Tag Manager,
    /// but we can still set them on objects that use built-in tags.
    /// NOTE: Custom tags (Enemy, PlayerBullet, etc.) must be added via
    /// ProjectSettings/TagManager.asset or the Unity Editor.
    /// </summary>
    private void SetupTags()
    {
        // Tags are configured in TagManager.asset — see ProjectSettings
    }

    private void CreateGameManager()
    {
        if (FindObjectOfType<GameManager>() != null) return;
        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    private void CreateUIManager()
    {
        if (FindObjectOfType<UIManager>() != null) return;
        GameObject go = new GameObject("UIManager");
        go.AddComponent<UIManager>();
    }

    private void CreateAudioManager()
    {
        if (FindObjectOfType<AudioManager>() != null) return;
        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
    }

    private void CreatePlayer()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null) return;

        GameObject player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(0, -3.5f, 0);

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreatePlayerShipSprite();
        sr.sortingOrder = 5;

        player.transform.localScale = Vector3.one * 0.8f;

        BoxCollider2D col = player.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.6f, 0.8f);

        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        PlayerController pc = player.AddComponent<PlayerController>();

        // Create player bullet prefab and assign it
        GameObject bulletPrefab = CreatePlayerBulletPrefab();

        // Use reflection or serialized field to assign — for simplicity,
        // we use a public setup method
        System.Reflection.FieldInfo bulletField = typeof(PlayerController)
            .GetField("bulletPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (bulletField != null)
            bulletField.SetValue(pc, bulletPrefab);
    }

    private GameObject CreatePlayerBulletPrefab()
    {
        GameObject prefab = new GameObject("PlayerBulletPrefab");
        prefab.tag = "PlayerBullet";

        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.CreateRectSprite(Color.green);
        sr.color = new Color(0.3f, 1f, 0.3f);
        sr.sortingOrder = 4;

        prefab.transform.localScale = new Vector3(0.15f, 0.4f, 1f);

        BoxCollider2D col = prefab.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        Rigidbody2D rb = prefab.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        prefab.AddComponent<BulletController>();

        prefab.SetActive(false);
        return prefab;
    }

    private void CreateEnemySpawner()
    {
        if (FindObjectOfType<EnemySpawner>() != null) return;
        GameObject go = new GameObject("EnemySpawner");
        go.AddComponent<EnemySpawner>();
    }

    private void CreateParallaxBackground()
    {
        if (FindObjectOfType<ParallaxBackground>() != null) return;
        GameObject go = new GameObject("ParallaxBackground");
        go.AddComponent<ParallaxBackground>();
    }
}
