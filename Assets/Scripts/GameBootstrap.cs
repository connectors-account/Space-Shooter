using UnityEngine;

/// <summary>
/// One-stop scene bootstrapper. Attach this to a single empty GameObject in
/// an otherwise empty scene and press Play — it builds the camera, background,
/// managers, and the player/enemy prefabs programmatically. This lets the whole
/// game run without any manual scene wiring, while still letting you assign
/// custom prefabs/art in the inspector if you prefer.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Optional Overrides")]
    [Tooltip("Assign a custom player prefab, or leave empty to auto-generate.")]
    public GameObject playerPrefabOverride;
    [Tooltip("Assign a custom enemy prefab, or leave empty to auto-generate.")]
    public GameObject enemyPrefabOverride;

    private void Awake()
    {
        SetupCamera();
        SetupBackground();

        GameObject player = playerPrefabOverride != null ? playerPrefabOverride : BuildPlayerPrefab();
        GameObject enemy = enemyPrefabOverride != null ? enemyPrefabOverride : BuildEnemyPrefab();

        SetupManagers(player, enemy);
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.08f);
        cam.clearFlags = CameraClearFlags.SolidColor;
    }

    private void SetupBackground()
    {
        GameObject bg = new GameObject("Background");
        bg.AddComponent<BackgroundScroller>();
    }

    private void SetupManagers(GameObject playerPrefab, GameObject enemyPrefab)
    {
        // UI first so the managers can find it.
        GameObject uiGO = new GameObject("UIManager");
        uiGO.AddComponent<UIManager>();

        GameObject gmGO = new GameObject("GameManager");
        GameManager gm = gmGO.AddComponent<GameManager>();
        gm.playerPrefab = playerPrefab;
        gm.enemyPrefab = enemyPrefab;
    }

    /// <summary>Builds a triangular player ship prefab (kept inactive as a template).</summary>
    private GameObject BuildPlayerPrefab()
    {
        GameObject go = new GameObject("Player");
        go.tag = "Player";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PrimitiveSprite.Triangle();
        sr.color = new Color(0.3f, 0.8f, 1f);
        sr.sortingOrder = 3;
        go.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        // Enable trigger detection between kinematic bodies (bullets/ships).
        rb.useFullKinematicContacts = true;

        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        go.AddComponent<PlayerController>();

        go.SetActive(false); // template; GameManager instantiates copies
        return go;
    }

    /// <summary>Builds an inverted-triangle enemy ship prefab (kept inactive).</summary>
    private GameObject BuildEnemyPrefab()
    {
        GameObject go = new GameObject("Enemy");
        go.tag = "Enemy";

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = PrimitiveSprite.Triangle();
        sr.color = new Color(1f, 0.4f, 0.4f);
        sr.sortingOrder = 3;
        // Flip vertically so it points downward.
        go.transform.localScale = new Vector3(0.8f, -0.8f, 1f);

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        // Enable trigger detection between kinematic bodies (bullets/ships).
        rb.useFullKinematicContacts = true;

        PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
        col.isTrigger = true;

        go.AddComponent<EnemyController>();

        go.SetActive(false);
        return go;
    }
}
