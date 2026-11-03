using System.Collections.Generic;
using UnityEngine;

// No scene references, prefabs, tags, imported art or physics setup required.
public sealed class SpaceShooter : MonoBehaviour
{
    private const float HalfWidth = 8f;
    private const float HalfHeight = 6f;
    private const float PlayerSpeed = 6f;
    private const float EnemySpeed = 2.6f;
    private const float BulletSpeed = 12f;
    private const float ShotInterval = 0.18f;
    private const float SpawnInterval = 0.85f;
    private static readonly Vector2 PlayerHalfSize = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 EnemyHalfSize = new Vector2(0.5f, 0.4f);
    private static readonly Vector2 BulletHalfSize = new Vector2(0.09f, 0.22f);

    private sealed class Body
    {
        public GameObject View;
        public Vector2 Position;
        public Vector2 Previous;
    }

    private readonly List<Body> enemies = new List<Body>();
    private readonly List<Body> bullets = new List<Body>();
    private Sprite square;
    private Transform player;
    private Camera gameCamera;
    private Vector2 playerPrevious;
    private float shotTimer;
    private float spawnTimer;
    private int score;
    private bool gameOver;
    private bool focused = true;
    private string lossReason = "";
    private GUIStyle hudStyle;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        new GameObject("Space Shooter").AddComponent<SpaceShooter>();
    }

    private void Awake()
    {
        // A built-in white texture becomes one shared, unlit, coloured rectangle.
        Texture2D texture = Texture2D.whiteTexture;
        square = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), texture.width, 0, SpriteMeshType.FullRect);
        square.name = "Runtime Rectangle";

        gameCamera = new GameObject("Game Camera").AddComponent<Camera>();
        gameCamera.transform.SetParent(transform);
        gameCamera.transform.position = new Vector3(0, 0, -10);
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = HalfHeight;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.08f);
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 20f;
        FitCamera();

        MakeRectangle("Left edge", transform, new Vector2(-HalfWidth, 0),
            new Vector2(0.05f, 12f), new Color(0.15f, 0.3f, 0.45f), -1);
        MakeRectangle("Right edge", transform, new Vector2(HalfWidth, 0),
            new Vector2(0.05f, 12f), new Color(0.15f, 0.3f, 0.45f), -1);
        MakeRectangle("Bottom danger line", transform, new Vector2(0, -5.8f),
            new Vector2(16f, 0.04f), new Color(0.8f, 0.2f, 0.25f), -1);

        player = new GameObject("Player").transform;
        player.SetParent(transform);
        MakeRectangle("Hull", player, Vector2.zero, new Vector2(0.4f, 0.8f), Color.cyan);
        MakeRectangle("Wings", player, new Vector2(0, -0.2f), new Vector2(1f, 0.25f), Color.cyan);
        MakeRectangle("Nose", player, new Vector2(0, 0.4f), new Vector2(0.2f, 0.2f), Color.white);
        RestartGame();
    }

    private void FitCamera()
    {
        // Keep the entire fixed playfield visible even in a tall or resized window.
        float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
        gameCamera.orthographicSize = Mathf.Max(HalfHeight, (HalfWidth + 0.2f) / aspect);
    }

    private void Update()
    {
        FitCamera();
        if (!focused) return;
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) RestartGame();
            return;
        }

        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        playerPrevious = player.position;
        Vector2 direction = Vector2.zero;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction.y -= 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction.x -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction.x += 1;
        Vector2 position = (Vector2)player.position + Vector2.ClampMagnitude(direction, 1f) * PlayerSpeed * dt;
        position.x = Mathf.Clamp(position.x, -HalfWidth + 0.55f, HalfWidth - 0.55f);
        position.y = Mathf.Clamp(position.y, -HalfHeight + 0.7f, HalfHeight - 0.7f);
        player.position = position;

        shotTimer -= dt;
        if (Input.GetKey(KeyCode.Space) && shotTimer <= 0)
        {
            Vector2 muzzle = position + Vector2.up * 0.75f;
            GameObject view = MakeRectangle("Shot", transform, muzzle,
                BulletHalfSize * 2f, new Color(1f, 0.85f, 0.25f));
            bullets.Add(new Body { View = view, Position = muzzle, Previous = muzzle });
            shotTimer = ShotInterval;
        }

        spawnTimer -= dt;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            spawnTimer = SpawnInterval;
        }
        MoveBodies(enemies, Vector2.down * EnemySpeed * dt);
        MoveBodies(bullets, Vector2.up * BulletSpeed * dt);

        // Shots resolve before player hits: destroying an enemy can save the ship.
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Body bullet = bullets[i];
            int hitIndex = -1;
            float firstHit = float.PositiveInfinity;
            for (int j = 0; j < enemies.Count; j++)
            {
                Body enemy = enemies[j];
                float hitTime;
                if (Sweep(bullet.Previous - enemy.Previous, bullet.Position - enemy.Position,
                    BulletHalfSize + EnemyHalfSize, out hitTime) && hitTime < firstHit)
                {
                    hitIndex = j;
                    firstHit = hitTime;
                }
            }
            if (hitIndex >= 0)
            {
                RemoveBody(enemies, hitIndex);
                RemoveBody(bullets, i);
                score += 10;
            }
            else if (bullet.Position.y > HalfHeight + 1f) RemoveBody(bullets, i);
        }

        foreach (Body enemy in enemies)
        {
            float hitTime;
            if (Sweep(enemy.Previous - playerPrevious, enemy.Position - (Vector2)player.position,
                EnemyHalfSize + PlayerHalfSize, out hitTime))
            {
                EndGame("Your ship was hit.");
                break;
            }
            if (enemy.Position.y - EnemyHalfSize.y <= -5.8f)
            {
                EndGame("An enemy reached the bottom.");
                break;
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector2 position = new Vector2(Random.Range(-7.35f, 7.35f), HalfHeight + 0.5f);
        Transform root = new GameObject("Enemy").transform;
        root.SetParent(transform);
        root.position = position;
        Color red = new Color(1f, 0.25f, 0.3f);
        MakeRectangle("Body", root, Vector2.zero, new Vector2(0.65f, 0.7f), red);
        MakeRectangle("Wings", root, new Vector2(0, 0.15f), new Vector2(1f, 0.25f), red);
        MakeRectangle("Core", root, new Vector2(0, -0.2f), new Vector2(0.25f, 0.3f), Color.white, 1);
        enemies.Add(new Body { View = root.gameObject, Position = position, Previous = position });
    }

    private GameObject MakeRectangle(string label, Transform parent, Vector2 position,
        Vector2 size, Color color, int order = 0)
    {
        GameObject view = new GameObject(label);
        view.transform.SetParent(parent, false);
        view.transform.localPosition = position;
        view.transform.localScale = new Vector3(size.x, size.y, 1);
        SpriteRenderer renderer = view.AddComponent<SpriteRenderer>();
        renderer.sprite = square;
        renderer.color = color;
        renderer.sortingOrder = order;
        return view;
    }

    private static void MoveBodies(List<Body> bodies, Vector2 displacement)
    {
        foreach (Body body in bodies)
        {
            body.Previous = body.Position;
            body.Position += displacement;
            body.View.transform.position = body.Position;
        }
    }

    // Segment versus expanded AABB: relative motion avoids fast-shot tunnelling.
    private static bool Sweep(Vector2 start, Vector2 end, Vector2 halfSize, out float hitTime)
    {
        float enter = 0f;
        float exit = 1f;
        Vector2 delta = end - start;
        for (int axis = 0; axis < 2; axis++)
        {
            if (Mathf.Abs(delta[axis]) < 0.00001f)
            {
                if (Mathf.Abs(start[axis]) > halfSize[axis]) { hitTime = 0f; return false; }
            }
            else
            {
                float a = (-halfSize[axis] - start[axis]) / delta[axis];
                float b = (halfSize[axis] - start[axis]) / delta[axis];
                enter = Mathf.Max(enter, Mathf.Min(a, b));
                exit = Mathf.Min(exit, Mathf.Max(a, b));
                if (enter > exit) { hitTime = 0f; return false; }
            }
        }
        hitTime = enter;
        return true;
    }

    private static void RemoveBody(List<Body> bodies, int index)
    {
        bodies[index].View.SetActive(false);
        Destroy(bodies[index].View);
        bodies.RemoveAt(index);
    }

    private void RestartGame()
    {
        for (int i = enemies.Count - 1; i >= 0; i--) RemoveBody(enemies, i);
        for (int i = bullets.Count - 1; i >= 0; i--) RemoveBody(bullets, i);
        player.position = new Vector3(0, -4.5f, 0);
        playerPrevious = player.position;
        score = 0;
        shotTimer = 0f;
        spawnTimer = 1.2f;
        lossReason = "";
        gameOver = false;
    }

    private void EndGame(string reason)
    {
        gameOver = true;
        lossReason = reason;
    }

    private void OnApplicationFocus(bool hasFocus) { focused = hasFocus; }

    private void OnGUI()
    {
        if (hudStyle == null)
        {
            hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            hudStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(hudStyle) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            textStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        }
        Matrix4x4 previousMatrix = GUI.matrix;
        float scale = Mathf.Min(Screen.width / 960f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960 * scale) / 2,
            (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
        GUI.Box(new Rect(12, 12, 215, 48), "");
        GUI.Label(new Rect(24, 18, 200, 40), "SCORE  " + score, hudStyle);
        GUI.Box(new Rect(230, 12, 718, 48), "");
        GUI.Label(new Rect(240, 18, 698, 36), "WASD / Arrows: move    |    Space: shoot    |    Stop every enemy", textStyle);
        if (gameOver)
        {
            GUI.Box(new Rect(240, 215, 480, 290), "");
            GUI.Label(new Rect(250, 230, 460, 55), "GAME OVER", titleStyle);
            GUI.Label(new Rect(250, 292, 460, 35), lossReason, textStyle);
            GUI.Label(new Rect(250, 332, 460, 35), "Final score: " + score, textStyle);
            if (GUI.Button(new Rect(330, 385, 300, 55), "Restart", buttonStyle)) RestartGame();
            GUI.Label(new Rect(250, 450, 460, 30), "Or press R / Enter", textStyle);
        }
        else if (!focused)
        {
            GUI.Box(new Rect(280, 300, 400, 80), "");
            GUI.Label(new Rect(290, 310, 380, 60), "Click the game to resume", textStyle);
        }
        GUI.matrix = previousMatrix;
    }

    private void OnDestroy()
    {
        if (square != null) Destroy(square);
        // The white texture belongs to Unity, so it must not be destroyed.
    }
}
