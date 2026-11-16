using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// One owner for the entire game. Collisions are explicit rectangles, not physics.
public sealed class SpaceShooterGame : MonoBehaviour
{
    private const float HalfWidth = 8f;
    private const float HalfHeight = 6f;
    private const float PlayerSpeed = 7f;
    private const float BulletSpeed = 16f;
    private const float EnemySpeed = 3f;
    private const float ShotInterval = 0.18f;
    private const float SpawnInterval = 0.85f;
    private static readonly Vector2 PlayerSize = new Vector2(1.2f, 0.8f);
    private static readonly Vector2 EnemySize = new Vector2(1.1f, 0.8f);
    private static readonly Vector2 BulletSize = new Vector2(0.16f, 0.5f);

    private sealed class Actor
    {
        public GameObject View;
        public Vector2 Position;

        public void Move(Vector2 position)
        {
            Position = position;
            View.transform.position = new Vector3(position.x, position.y, 0f);
        }
    }

    private readonly List<Actor> enemies = new List<Actor>();
    private readonly List<Actor> bullets = new List<Actor>();
    private Actor player;
    private Material playerMaterial;
    private Material enemyMaterial;
    private Material bulletMaterial;
    private Camera gameCamera;
    private float shotTimer;
    private float spawnTimer;
    private int score;
    private bool gameOver;
    private bool paused;
    private GUIStyle headingStyle;
    private GUIStyle labelStyle;
    private GUIStyle buttonStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name == "Main")
            new GameObject("Space Shooter").AddComponent<SpaceShooterGame>();
    }

    private void Awake()
    {
        playerMaterial = Resources.Load<Material>("Player");
        enemyMaterial = Resources.Load<Material>("Enemy");
        bulletMaterial = Resources.Load<Material>("Bullet");
        if (playerMaterial == null || enemyMaterial == null || bulletMaterial == null)
        {
            Debug.LogError("Missing game materials. Run Space Shooter > Setup / Open Main in the editor.");
            enabled = false;
            return;
        }

        gameCamera = new GameObject("Game Camera").AddComponent<Camera>();
        gameCamera.transform.SetParent(transform);
        gameCamera.transform.position = new Vector3(0f, 0f, -10f);
        gameCamera.orthographic = true;
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 30f;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.015f, 0.025f, 0.065f);
        Application.targetFrameRate = 60;
        StartRound();
    }

    private Actor CreateActor(string actorName, Vector2 position, Vector2 size, Material material)
    {
        GameObject view = GameObject.CreatePrimitive(PrimitiveType.Cube);
        view.name = actorName;
        view.transform.SetParent(transform);
        view.transform.localScale = new Vector3(size.x, size.y, 0.2f);
        // Cube primitives include colliders. This game uses its own swept checks.
        Destroy(view.GetComponent<Collider>());
        view.GetComponent<Renderer>().sharedMaterial = material;
        Actor actor = new Actor { View = view };
        actor.Move(position);
        return actor;
    }

    private void StartRound()
    {
        ClearActors(enemies);
        ClearActors(bullets);
        if (player != null) Destroy(player.View);
        player = CreateActor("Player ship", new Vector2(0f, -4.5f), PlayerSize, playerMaterial);
        // A small raised cockpit gives the rectangle a ship silhouette.
        GameObject cockpit = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cockpit.name = "Cockpit";
        cockpit.transform.SetParent(player.View.transform, false);
        cockpit.transform.localPosition = new Vector3(0f, 0.25f, -0.6f);
        cockpit.transform.localScale = new Vector3(0.3f, 0.5f, 1f);
        cockpit.GetComponent<Renderer>().sharedMaterial = bulletMaterial;
        Destroy(cockpit.GetComponent<Collider>());
        score = 0;
        shotTimer = 0f;
        spawnTimer = 1.5f;
        gameOver = false;
        paused = false;
    }

    private static void ClearActors(List<Actor> actors)
    {
        foreach (Actor actor in actors) Destroy(actor.View);
        actors.Clear();
    }

    private void Update()
    {
        // Preserve the entire play area, including when the window is resized.
        gameCamera.orthographicSize = Mathf.Max(HalfHeight, HalfWidth / gameCamera.aspect);
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) StartRound();
            return;
        }
        if (Input.GetKeyDown(KeyCode.P)) paused = !paused;
        if (paused) return;

        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        Vector2 movement = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement.x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement.y -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement.y += 1f;
        Vector2 next = player.Position + Vector2.ClampMagnitude(movement, 1f) * (PlayerSpeed * dt);
        next.x = Mathf.Clamp(next.x, -HalfWidth + PlayerSize.x / 2f, HalfWidth - PlayerSize.x / 2f);
        next.y = Mathf.Clamp(next.y, -HalfHeight + PlayerSize.y / 2f, HalfHeight - PlayerSize.y / 2f);
        player.Move(next);

        shotTimer = Mathf.Max(0f, shotTimer - dt);
        if (Input.GetKey(KeyCode.Space) && shotTimer <= 0f)
        {
            bullets.Add(CreateActor("Shot", player.Position + Vector2.up * 0.8f, BulletSize, bulletMaterial));
            shotTimer = ShotInterval;
        }
        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            float x = Random.Range(-HalfWidth + EnemySize.x / 2f, HalfWidth - EnemySize.x / 2f);
            enemies.Add(CreateActor("Enemy", new Vector2(x, HalfHeight + 0.5f), EnemySize, enemyMaterial));
            spawnTimer = SpawnInterval;
        }

        // Check bullets before enemy movement. Extend the bullet sweep by the
        // enemy's downward travel, so fast opposing objects cannot tunnel.
        for (int b = bullets.Count - 1; b >= 0; b--)
        {
            Actor bullet = bullets[b];
            float travel = BulletSpeed * dt;
            Vector2 sweepCenter = bullet.Position + Vector2.up * ((travel + EnemySpeed * dt) / 2f);
            Vector2 sweepSize = BulletSize + Vector2.up * (travel + EnemySpeed * dt);
            bool hit = false;
            // Enemies are stored oldest first: hit the nearest oncoming enemy first.
            for (int e = 0; e < enemies.Count; e++)
            {
                if (!Overlaps(sweepCenter, sweepSize, enemies[e].Position, EnemySize)) continue;
                Destroy(enemies[e].View);
                enemies.RemoveAt(e);
                score += 10;
                hit = true;
                break;
            }
            bullet.Move(bullet.Position + Vector2.up * travel);
            if (hit || bullet.Position.y > HalfHeight + 1f)
            {
                Destroy(bullet.View);
                bullets.RemoveAt(b);
            }
        }

        foreach (Actor enemy in enemies)
        {
            enemy.Move(enemy.Position + Vector2.down * (EnemySpeed * dt));
            if (Overlaps(player.Position, PlayerSize, enemy.Position, EnemySize)
                || enemy.Position.y < -HalfHeight - EnemySize.y / 2f)
            {
                gameOver = true;
                break;
            }
        }
    }

    private static bool Overlaps(Vector2 a, Vector2 aSize, Vector2 b, Vector2 bSize)
    {
        return Mathf.Abs(a.x - b.x) <= (aSize.x + bSize.x) / 2f
            && Mathf.Abs(a.y - b.y) <= (aSize.y + bSize.y) / 2f;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (!focused && !gameOver) paused = true;
    }

    private void OnGUI()
    {
        if (player == null) return;
        if (headingStyle == null)
        {
            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 32, alignment = TextAnchor.MiddleCenter };
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
        }
        // Fixed virtual UI size keeps labels readable at common window resolutions.
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 960f, Screen.height / 720f, 1f));
        GUI.Box(new Rect(10f, 10f, 940f, 72f), GUIContent.none);
        GUI.Label(new Rect(20f, 12f, 920f, 32f), "SPACE SHOOTER  |  SCORE " + score, labelStyle);
        GUI.Label(new Rect(20f, 44f, 920f, 28f), "WASD / Arrows: move   Space: fire   P: pause   Esc: quit", labelStyle);
        if (gameOver || paused)
        {
            GUI.Box(new Rect(230f, 235f, 500f, 250f), GUIContent.none);
            GUI.Label(new Rect(250f, 250f, 460f, 50f), gameOver ? "GAME OVER" : "PAUSED", headingStyle);
            GUI.Label(new Rect(250f, 310f, 460f, 32f), gameOver ? "Final score: " + score : "Press P or click Resume", labelStyle);
            GUI.Label(new Rect(250f, 345f, 460f, 32f), gameOver ? "An enemy hit you or escaped." : "Stop every enemy before it escapes.", labelStyle);
            if (GUI.Button(new Rect(330f, 400f, 300f, 55f), gameOver ? "Restart (R / Enter)" : "Resume", buttonStyle))
            {
                if (gameOver) StartRound();
                else paused = false;
            }
        }
        GUI.matrix = oldMatrix;
    }
}
