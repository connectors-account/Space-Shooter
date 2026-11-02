using System.Collections.Generic;
using UnityEngine;

// Everything visible is created here. Collisions are explicit 2D bounds tests;
// no prefabs, sprites, physics setup, input mappings, or audio are required.
public sealed class SpaceShooter : MonoBehaviour
{
    private const float HalfWidth = 10.666667f;
    private const float HalfHeight = 6f;
    private const float PlayerSpeed = 7f;
    private const float BulletSpeed = 15f;
    private const float EnemySpeed = 3f;
    private const float SpawnInterval = 0.85f;
    private const float FireInterval = 0.18f;
    private static readonly Vector2 PlayerSize = new Vector2(0.85f, 0.8f);
    private static readonly Vector2 EnemySize = new Vector2(0.9f, 0.8f);
    private static readonly Vector2 BulletSize = new Vector2(0.14f, 0.48f);

    private readonly List<Transform> bullets = new List<Transform>();
    private readonly List<Transform> enemies = new List<Transform>();
    private readonly List<Material> materials = new List<Material>();
    private Transform player;
    private Camera gameCamera;
    private Material playerMaterial, enemyMaterial, bulletMaterial, cockpitMaterial;
    private float spawnTimer, fireTimer;
    private int score;
    private bool gameOver, paused;
    private GUIStyle titleStyle, labelStyle, buttonStyle;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Shader shader = Resources.Load<Shader>("Flat");
        if (shader == null)
        {
            Debug.LogError("Missing Resources/Flat.shader. Restore the supplied shader.", this);
            enabled = false;
            return;
        }
        playerMaterial = MakeMaterial(shader, new Color(0.15f, 0.85f, 1f));
        enemyMaterial = MakeMaterial(shader, new Color(1f, 0.28f, 0.37f));
        bulletMaterial = MakeMaterial(shader, new Color(1f, 0.88f, 0.3f));
        cockpitMaterial = MakeMaterial(shader, new Color(0.03f, 0.12f, 0.22f));

        GameObject cameraObject = new GameObject("Game Camera");
        cameraObject.transform.SetParent(transform, false);
        gameCamera = cameraObject.AddComponent<Camera>();
        gameCamera.transform.position = new Vector3(0f, 0f, -10f);
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = HalfHeight;
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 30f;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.025f, 0.04f, 0.09f);
        gameCamera.allowHDR = false;
        // Intentionally no AudioListener: this is a silent game.
        UpdateViewport();
        Restart();
    }

    private Material MakeMaterial(Shader shader, Color color)
    {
        Material material = new Material(shader) { color = color };
        materials.Add(material);
        return material;
    }

    private Transform MakeShape(string objectName, Vector2 position, Vector2 size,
                                Material material, Transform parent = null)
    {
        GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Quad);
        shape.name = objectName;
        shape.transform.SetParent(parent != null ? parent : transform, false);
        shape.transform.localPosition = new Vector3(position.x, position.y, 0f);
        shape.transform.localScale = new Vector3(size.x, size.y, 1f);
        // Explicit component references also keep primitive support in player builds.
        if (shape.GetComponent<MeshFilter>() == null)
            throw new System.InvalidOperationException("The built-in quad mesh is unavailable.");
        shape.GetComponent<MeshRenderer>().sharedMaterial = material;
        Destroy(shape.GetComponent<MeshCollider>());
        return shape.transform;
    }

    private Transform MakeShip(string objectName, Vector2 position, Vector2 size, Material material)
    {
        Transform ship = MakeShape(objectName, position, size, material);
        Transform cockpit = MakeShape("Cockpit", new Vector2(0f, 0.12f),
                                      new Vector2(0.35f, 0.4f), cockpitMaterial, ship);
        cockpit.localPosition += Vector3.back * 0.02f;
        return ship;
    }

    private void Restart()
    {
        ClearObjects(bullets);
        ClearObjects(enemies);
        if (player != null) Destroy(player.gameObject);
        player = MakeShip("Player", new Vector2(0f, -4.5f), PlayerSize, playerMaterial);
        score = 0;
        spawnTimer = 1.2f;
        fireTimer = 0f;
        gameOver = false;
        paused = false;
    }

    private static void ClearObjects(List<Transform> objects)
    {
        foreach (Transform item in objects) Destroy(item.gameObject);
        objects.Clear();
    }

    private void UpdateViewport()
    {
        float screenAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
        float targetAspect = 16f / 9f;
        if (screenAspect > targetAspect)
        {
            float width = targetAspect / screenAspect;
            gameCamera.rect = new Rect((1f - width) / 2f, 0f, width, 1f);
        }
        else
        {
            float height = screenAspect / targetAspect;
            gameCamera.rect = new Rect(0f, (1f - height) / 2f, 1f, height);
        }
        gameCamera.aspect = targetAspect;
    }

    private void Update()
    {
        UpdateViewport();
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) Restart();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P)) paused = !paused;
        if (paused) return;

        // Clamp long frames so returning to a stalled window cannot skip collisions.
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
        Vector2 direction = new Vector2(
            (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f),
            (Held(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f));
        Vector3 position = player.position + (Vector3)Vector2.ClampMagnitude(direction, 1f) * (PlayerSpeed * dt);
        position.x = Mathf.Clamp(position.x, -HalfWidth + 0.5f, HalfWidth - 0.5f);
        position.y = Mathf.Clamp(position.y, -HalfHeight + 0.5f, HalfHeight - 1.2f);
        player.position = position;

        fireTimer -= dt;
        if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
        {
            bullets.Add(MakeShape("Bullet", (Vector2)position + Vector2.up * 0.7f, BulletSize, bulletMaterial));
            fireTimer = FireInterval;
        }
        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            enemies.Add(MakeShip("Enemy", new Vector2(Random.Range(-HalfWidth + 0.6f, HalfWidth - 0.6f),
                                                      HalfHeight + 0.6f), EnemySize, enemyMaterial));
            spawnTimer = SpawnInterval;
        }
        foreach (Transform enemy in enemies) enemy.position += Vector3.down * (EnemySpeed * dt);

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Transform bullet = bullets[i];
            Vector2 oldPosition = bullet.position;
            bullet.position += Vector3.up * (BulletSpeed * dt);
            bool hit = false;
            // Sweep the relative vertical motion of bullet and enemy to prevent tunnelling.
            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                Vector2 enemyPosition = enemies[j].position;
                float radiusX = (BulletSize.x + EnemySize.x) * 0.5f;
                float radiusY = (BulletSize.y + EnemySize.y) * 0.5f;
                float previousRelativeY = oldPosition.y - (enemyPosition.y + EnemySpeed * dt);
                float currentRelativeY = bullet.position.y - enemyPosition.y;
                if (Mathf.Abs(bullet.position.x - enemyPosition.x) <= radiusX &&
                    previousRelativeY <= radiusY && currentRelativeY >= -radiusY)
                {
                    Destroy(enemies[j].gameObject);
                    enemies.RemoveAt(j);
                    score += 10;
                    hit = true;
                    break;
                }
            }
            if (hit || bullet.position.y > HalfHeight + 1f)
            {
                Destroy(bullet.gameObject);
                bullets.RemoveAt(i);
            }
        }
        foreach (Transform enemy in enemies)
        {
            Vector2 difference = enemy.position - player.position;
            bool touchesPlayer = Mathf.Abs(difference.x) < (PlayerSize.x + EnemySize.x) / 2f &&
                                 Mathf.Abs(difference.y) < (PlayerSize.y + EnemySize.y) / 2f;
            if (touchesPlayer || enemy.position.y < -HalfHeight - EnemySize.y / 2f)
            {
                gameOver = true;
                break;
            }
        }
    }

    private static bool Held(KeyCode first, KeyCode second)
    {
        return Input.GetKey(first) || Input.GetKey(second);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && !gameOver) paused = true;
    }

    private void OnGUI()
    {
        if (gameCamera == null) return;
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(labelStyle) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        }
        Matrix4x4 oldMatrix = GUI.matrix;
        Rect viewport = gameCamera.pixelRect;
        float scale = viewport.width / 960f;
        GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.x, Screen.height - viewport.yMax, 0f),
                                  Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.Box(new Rect(0, 0, 960, 48), GUIContent.none);
        GUI.Label(new Rect(18, 8, 260, 36), "SCORE  " + score, labelStyle);
        GUI.Label(new Rect(300, 8, 650, 36), "WASD / Arrows: move    Space: fire    P: pause", labelStyle);
        if (gameOver || paused)
        {
            GUI.Box(new Rect(160, 140, 640, 290), GUIContent.none);
            GUI.Label(new Rect(180, 158, 600, 70), gameOver ? "GAME OVER" : "PAUSED", titleStyle);
            GUIStyle centered = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(180, 238, 600, 40), gameOver ? "Final score: " + score : "Press P to resume", centered);
            GUI.Label(new Rect(180, 282, 600, 40), "Stop enemies before they reach the bottom.", centered);
            if (GUI.Button(new Rect(340, 344, 280, 54), gameOver ? "Restart (R / Enter)" : "Resume", buttonStyle))
            {
                if (gameOver) Restart();
                else paused = false;
            }
        }
        GUI.matrix = oldMatrix;
    }

    private void OnDestroy()
    {
        foreach (Material material in materials) Destroy(material);
    }
}
