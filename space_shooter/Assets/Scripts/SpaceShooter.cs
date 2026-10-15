using System.Collections.Generic;
using UnityEngine;

// All gameplay is created at runtime; the editor setup supplies only this
// component and a shader-backed material, so no prefab wiring is needed.
public sealed class SpaceShooter : MonoBehaviour
{
    [SerializeField] private Material shapeMaterial;

    private const float HalfWidth = 8f;
    private const float HalfHeight = 4.5f;
    private const float PlayerSpeed = 6f;
    private const float BulletSpeed = 16f;
    private const float EnemySpeed = 2.5f;
    private const float FireInterval = 0.18f;
    private const float SpawnInterval = 0.8f;

    private sealed class Body
    {
        public Transform Transform;
        public Vector2 Previous;
        public Vector2 HalfSize;
        public Vector2 Position => Transform.position;
    }

    private readonly List<Body> bullets = new List<Body>();
    private readonly List<Body> enemies = new List<Body>();
    private Body player;
    private Camera gameCamera;
    private Sprite square;
    private Vector2 movement;
    private bool firing;
    private bool gameOver;
    private bool hasFocus = true;
    private float fireTimer;
    private float spawnTimer;
    private int score;
    private GUIStyle labelStyle;
    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        if (shapeMaterial == null)
        {
            Debug.LogError("Missing shape material. Run Space Shooter > Open / Create Game Scene.", this);
            enabled = false;
            return;
        }

        square = Sprite.Create(Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f), Texture2D.whiteTexture.width);
        square.name = "Runtime square";

        var cameraObject = new GameObject("Game Camera");
        cameraObject.transform.SetParent(transform);
        cameraObject.transform.position = new Vector3(0, 0, -10);
        gameCamera = cameraObject.AddComponent<Camera>();
        gameCamera.orthographic = true;
        gameCamera.orthographicSize = HalfHeight;
        gameCamera.nearClipPlane = 0.1f;
        gameCamera.farClipPlane = 20f;
        gameCamera.clearFlags = CameraClearFlags.SolidColor;
        gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.075f);
        FitCamera();

        player = CreateBody("Player", Vector2.zero, new Vector2(0.45f, 0.45f));
        AddShape(player, new Vector2(0, 0.08f), new Vector2(0.34f, 0.74f), Color.cyan);
        AddShape(player, new Vector2(0, -0.22f), new Vector2(0.9f, 0.34f), Color.cyan);
        AddShape(player, new Vector2(0, 0.16f), new Vector2(0.15f, 0.24f), Color.white);
        Restart();
    }

    private void Update()
    {
        FitCamera();
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
                Restart();
            return;
        }

        float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0)
                - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
        float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0)
                - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
        movement = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
        firing = Input.GetKey(KeyCode.Space);
    }

    private static bool Held(KeyCode first, KeyCode second)
    {
        return Input.GetKey(first) || Input.GetKey(second);
    }

    private void FixedUpdate()
    {
        if (gameOver || !hasFocus || player == null)
            return;

        float dt = Time.fixedDeltaTime;
        player.Previous = player.Position;
        Vector2 position = player.Position + movement * (PlayerSpeed * dt);
        position.x = Mathf.Clamp(position.x, -HalfWidth + player.HalfSize.x, HalfWidth - player.HalfSize.x);
        position.y = Mathf.Clamp(position.y, -HalfHeight + player.HalfSize.y, HalfHeight - player.HalfSize.y);
        player.Transform.position = position;

        fireTimer = Mathf.Max(0, fireTimer - dt);
        if (firing && fireTimer <= 0)
        {
            Body bullet = CreateBody("Bullet", position + new Vector2(0, 0.6f), new Vector2(0.07f, 0.2f));
            AddShape(bullet, Vector2.zero, bullet.HalfSize * 2, new Color(1f, 0.9f, 0.3f));
            bullets.Add(bullet);
            fireTimer = FireInterval;
        }

        spawnTimer -= dt;
        if (spawnTimer <= 0)
        {
            SpawnEnemy();
            spawnTimer = SpawnInterval;
        }

        foreach (Body bullet in bullets)
            Move(bullet, Vector2.up * (BulletSpeed * dt));
        foreach (Body enemy in enemies)
            Move(enemy, Vector2.down * (EnemySpeed * dt));

        // Resolve shots first, then surviving enemies against the ship.
        // Swept bounds use relative motion, so fast bullets cannot skip a target.
        for (int b = bullets.Count - 1; b >= 0; b--)
        {
            for (int e = enemies.Count - 1; e >= 0; e--)
            {
                if (!SweptOverlap(bullets[b], enemies[e]))
                    continue;
                RemoveAt(bullets, b);
                RemoveAt(enemies, e);
                score += 10;
                break;
            }
        }

        foreach (Body enemy in enemies)
        {
            if (!SweptOverlap(player, enemy))
                continue;
            gameOver = true;
            movement = Vector2.zero;
            firing = false;
            player.Transform.gameObject.SetActive(false);
            break;
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
            if (bullets[i].Position.y - bullets[i].HalfSize.y > HalfHeight)
                RemoveAt(bullets, i);
        for (int i = enemies.Count - 1; i >= 0; i--)
            if (enemies[i].Position.y + enemies[i].HalfSize.y < -HalfHeight)
                RemoveAt(enemies, i);
    }

    private void SpawnEnemy()
    {
        Body enemy = CreateBody("Enemy", new Vector2(Random.Range(-7.4f, 7.4f), 5f), new Vector2(0.5f, 0.4f));
        Color red = new Color(1f, 0.25f, 0.35f);
        AddShape(enemy, new Vector2(0, 0.1f), new Vector2(1f, 0.35f), red);
        AddShape(enemy, new Vector2(0, -0.15f), new Vector2(0.5f, 0.5f), red);
        AddShape(enemy, new Vector2(0, -0.17f), new Vector2(0.18f, 0.15f), Color.white);
        enemies.Add(enemy);
    }

    private Body CreateBody(string name, Vector2 position, Vector2 halfSize)
    {
        var root = new GameObject(name);
        root.transform.SetParent(transform);
        root.transform.position = position;
        return new Body { Transform = root.transform, Previous = position, HalfSize = halfSize };
    }

    private void AddShape(Body body, Vector2 offset, Vector2 size, Color color)
    {
        var part = new GameObject("Shape");
        part.transform.SetParent(body.Transform, false);
        part.transform.localPosition = offset;
        part.transform.localScale = new Vector3(size.x, size.y, 1);
        var renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = square;
        renderer.sharedMaterial = shapeMaterial;
        renderer.color = color;
        renderer.sortingOrder = body.Transform.childCount;
    }

    private static void Move(Body body, Vector2 delta)
    {
        body.Previous = body.Position;
        body.Transform.position = body.Position + delta;
    }

    private static bool SweptOverlap(Body a, Body b)
    {
        Vector2 start = a.Previous - b.Previous;
        Vector2 delta = (a.Position - a.Previous) - (b.Position - b.Previous);
        Vector2 extent = a.HalfSize + b.HalfSize;
        float enter = 0f;
        float exit = 1f;
        for (int axis = 0; axis < 2; axis++)
        {
            if (Mathf.Abs(delta[axis]) < 0.000001f)
            {
                if (Mathf.Abs(start[axis]) > extent[axis])
                    return false;
                continue;
            }
            float t1 = (-extent[axis] - start[axis]) / delta[axis];
            float t2 = (extent[axis] - start[axis]) / delta[axis];
            enter = Mathf.Max(enter, Mathf.Min(t1, t2));
            exit = Mathf.Min(exit, Mathf.Max(t1, t2));
            if (enter > exit)
                return false;
        }
        return true;
    }

    private static void RemoveAt(List<Body> bodies, int index)
    {
        bodies[index].Transform.gameObject.SetActive(false);
        Destroy(bodies[index].Transform.gameObject);
        bodies.RemoveAt(index);
    }

    private void Restart()
    {
        for (int i = bullets.Count - 1; i >= 0; i--) RemoveAt(bullets, i);
        for (int i = enemies.Count - 1; i >= 0; i--) RemoveAt(enemies, i);
        player.Transform.position = new Vector2(0, -3.5f);
        player.Previous = player.Position;
        player.Transform.gameObject.SetActive(true);
        movement = Vector2.zero;
        firing = false;
        score = 0;
        fireTimer = 0;
        spawnTimer = 1f;
        gameOver = false;
    }

    private void FitCamera()
    {
        if (gameCamera == null || Screen.height == 0) return;
        const float target = 16f / 9f;
        float aspect = (float)Screen.width / Screen.height;
        gameCamera.rect = aspect > target
            ? new Rect((1f - target / aspect) / 2f, 0, target / aspect, 1)
            : new Rect(0, (1f - aspect / target) / 2f, 1, aspect / target);
        gameCamera.aspect = target;
    }

    private void OnApplicationFocus(bool focused)
    {
        hasFocus = focused;
        movement = Vector2.zero;
        firing = false;
    }

    private void OnGUI()
    {
        if (player == null) return;
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            labelStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(labelStyle) { fontSize = 40, fontStyle = FontStyle.Bold };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        }
        Matrix4x4 previousMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 960f, Screen.height / 540f, 1));
        GUI.Box(new Rect(16, 16, 170, 42), GUIContent.none);
        GUI.Label(new Rect(16, 16, 170, 42), "Score: " + score, labelStyle);
        GUI.Box(new Rect(180, 492, 600, 36), GUIContent.none);
        GUI.Label(new Rect(180, 492, 600, 36), "WASD / arrows: move     Space: fire", labelStyle);
        if (gameOver)
        {
            GUI.Box(new Rect(260, 140, 440, 270), GUIContent.none);
            GUI.Label(new Rect(280, 160, 400, 60), "GAME OVER", titleStyle);
            GUI.Label(new Rect(280, 225, 400, 40), "Final score: " + score, labelStyle);
            if (GUI.Button(new Rect(340, 285, 280, 55), "Restart", buttonStyle)) Restart();
            GUI.Label(new Rect(280, 350, 400, 35), "or press R / Enter", labelStyle);
        }
        GUI.matrix = previousMatrix;
    }

    private void OnDestroy()
    {
        if (square != null) Destroy(square);
    }
}
