using UnityEngine;

namespace MinimalSpaceShooter
{
    // The included scene needs no manually wired objects or prefabs.
    public sealed class SpaceShooter : MonoBehaviour
    {
        public const float HalfWidth = 8f;
        public const float HalfHeight = 6f;
        public bool Running { get; private set; }
        public int Score { get; private set; }

        private Transform roundRoot;
        private Sprite square;
        private float spawnCountdown;
        private string gameOverReason;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindObjectOfType<SpaceShooter>() == null)
                new GameObject("Space Shooter").AddComponent<SpaceShooter>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;

            // White texture + SpriteRenderer tint = plain coloured rectangles, not sprite art.
            Texture2D white = Texture2D.whiteTexture;
            square = Sprite.Create(white, new Rect(0, 0, white.width, white.height),
                new Vector2(0.5f, 0.5f), white.width);
            square.name = "Runtime White Square";

            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera").AddComponent<Camera>();
                camera.tag = "MainCamera";
            }
            camera.transform.SetPositionAndRotation(new Vector3(0, 0, -10), Quaternion.identity);
            camera.orthographic = true;
            camera.orthographicSize = HalfHeight;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.08f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 20f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            // Fixed 4:3 arena, letterboxed rather than changing the gameplay bounds.
            camera.gameObject.AddComponent<ArenaViewport>();
            StartRound();
        }

        public void StartRound()
        {
            Running = false;
            if (roundRoot != null)
            {
                // Destroy is deferred; disable first so old colliders cannot score this round.
                roundRoot.gameObject.SetActive(false);
                Destroy(roundRoot.gameObject);
            }
            roundRoot = new GameObject("Round").transform;
            roundRoot.SetParent(transform);
            Score = 0;
            gameOverReason = string.Empty;
            spawnCountdown = 1.2f;
            Running = true;

            GameObject player = CreateRectangle("Player", new Vector2(0, -4.7f),
                new Vector2(0.8f, 0.8f), Color.cyan, RigidbodyType2D.Dynamic);
            player.AddComponent<PlayerShip>().Initialize(this);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            if (!Running)
            {
                if (Input.GetKeyDown(KeyCode.R))
                    StartRound();
                return;
            }

            spawnCountdown -= Time.deltaTime;
            if (spawnCountdown <= 0f)
            {
                spawnCountdown = 0.9f;
                float x = Random.Range(-HalfWidth + 0.6f, HalfWidth - 0.6f);
                GameObject enemy = CreateRectangle("Enemy", new Vector2(x, HalfHeight + 0.5f),
                    new Vector2(0.9f, 0.9f), new Color(1f, 0.25f, 0.3f), RigidbodyType2D.Kinematic);
                enemy.AddComponent<EnemyShip>().Initialize(this);
            }
        }

        public void Shoot(Vector2 position)
        {
            if (!Running) return;
            GameObject bullet = CreateRectangle("Bullet", position,
                new Vector2(0.16f, 0.4f), Color.yellow, RigidbodyType2D.Dynamic);
            bullet.AddComponent<Projectile>().Initialize(this);
        }

        private GameObject CreateRectangle(string objectName, Vector2 position,
            Vector2 size, Color color, RigidbodyType2D bodyType)
        {
            GameObject shape = new GameObject(objectName);
            shape.transform.SetParent(roundRoot);
            shape.transform.position = position;
            shape.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = shape.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            BoxCollider2D collider = shape.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            collider.isTrigger = true;
            Rigidbody2D body = shape.AddComponent<Rigidbody2D>();
            body.bodyType = bodyType;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            return shape;
        }

        public void AddPoint()
        {
            if (Running) Score++;
        }

        public void EndRound(string reason)
        {
            if (!Running) return;
            Running = false;
            gameOverReason = reason;
            // Freeze immediately, including any later callbacks in the same physics step.
            foreach (Rigidbody2D body in roundRoot.GetComponentsInChildren<Rigidbody2D>())
                body.simulated = false;
        }

        private void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
                labelStyle.normal.textColor = Color.white;
                titleStyle = new GUIStyle(labelStyle) { fontSize = 32, fontStyle = FontStyle.Bold };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
            }

            // Scale UI with the window; no Canvas, UI package or font asset is required.
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(
                new Vector3((Screen.width - 960f * scale) / 2f,
                    (Screen.height - 720f * scale) / 2f, 0),
                Quaternion.identity, Vector3.one * scale);
            GUI.Box(new Rect(10, 10, 150, 44), GUIContent.none);
            GUI.Label(new Rect(10, 10, 150, 44), "Score: " + Score, labelStyle);
            GUI.Label(new Rect(165, 10, 785, 44),
                "WASD / Arrows: move   |   Space: shoot   |   Esc: quit", labelStyle);

            if (!Running)
            {
                GUI.Box(new Rect(220, 220, 520, 280), GUIContent.none);
                GUI.Label(new Rect(240, 240, 480, 50), "GAME OVER", titleStyle);
                GUI.Label(new Rect(240, 295, 480, 50), gameOverReason, labelStyle);
                GUI.Label(new Rect(240, 345, 480, 40), "Final score: " + Score, labelStyle);
                if (GUI.Button(new Rect(340, 410, 280, 56), "Restart (R)", buttonStyle))
                    StartRound();
            }
            GUI.matrix = previous;
        }

        private void OnDestroy()
        {
            if (square != null) Destroy(square);
        }
    }

}
