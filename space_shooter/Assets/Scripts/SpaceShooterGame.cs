using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private sealed class Actor
        {
            public Transform View;
            public Vector2 Position;
            public Vector2 Previous;
        }

        private readonly List<Actor> enemies = new List<Actor>();
        private readonly List<Actor> bullets = new List<Actor>();
        private readonly List<Material> materials = new List<Material>();
        private Transform player;
        private Vector2 playerPosition;
        private Camera gameCamera;
        private Material shipMaterial;
        private Material wingMaterial;
        private Material enemyMaterial;
        private Material bulletMaterial;
        private float spawnTimer;
        private float fireTimer;
        private int score;
        private bool gameOver;
        private bool focused = true;
        private GUIStyle titleStyle;
        private GUIStyle textStyle;
        private GUIStyle scoreStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<SpaceShooterGame>() == null)
                new GameObject("Space Shooter").AddComponent<SpaceShooterGame>();
        }

        private void Awake()
        {
            // This is a self-contained, single-scene game. Disable any template camera.
            foreach (Camera existing in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
                existing.enabled = false;

            gameCamera = new GameObject("Game Camera").AddComponent<Camera>();
            gameCamera.transform.SetParent(transform);
            gameCamera.transform.position = new Vector3(0f, 0f, -20f);
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = GameRules.ArenaHalfHeight;
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 50f;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.075f);
            gameCamera.tag = "MainCamera";
            // No AudioListener or audio objects: this game is intentionally silent.
            shipMaterial = CreateMaterial(new Color(0.2f, 0.85f, 1f));
            wingMaterial = CreateMaterial(new Color(0.1f, 0.45f, 0.75f));
            enemyMaterial = CreateMaterial(new Color(1f, 0.25f, 0.3f));
            bulletMaterial = CreateMaterial(new Color(1f, 0.9f, 0.3f));

            player = new GameObject("Player Ship").transform;
            player.SetParent(transform);
            CreateBox("Hull", player, Vector3.zero, new Vector3(0.4f, 1f, 0.3f), shipMaterial);
            CreateBox("Wings", player, new Vector3(0f, -0.15f, 0f),
                new Vector3(1.1f, 0.3f, 0.3f), wingMaterial);
            RestartGame();
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Resources.Load<Shader>("SolidColor");
            if (shader == null)
                throw new System.InvalidOperationException("Missing Resources/SolidColor.shader.");
            Material material = new Material(shader) { color = color };
            materials.Add(material);
            return material;
        }

        private static Transform CreateBox(string objectName, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = objectName;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = position;
            box.transform.localScale = scale;
            // Collision is deterministic swept AABB logic below, not Rigidbody physics.
            BoxCollider collider = box.GetComponent<BoxCollider>();
            collider.enabled = false;
            Object.Destroy(collider);
            MeshFilter mesh = box.GetComponent<MeshFilter>();
            if (mesh.sharedMesh == null)
                throw new System.InvalidOperationException("Unity cube mesh is unavailable.");
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            return box.transform;
        }

        private Actor CreateActor(string actorName, Vector2 position, Vector3 scale, Material material)
        {
            Transform view = CreateBox(actorName, transform,
                new Vector3(position.x, position.y, 0f), scale, material);
            return new Actor { View = view, Position = position, Previous = position };
        }

        private void Update()
        {
            // Keep the arena fixed when resizing the desktop window; add visible margin
            // rather than cropping gameplay on narrow aspect ratios.
            gameCamera.orthographicSize = Mathf.Max(GameRules.ArenaHalfHeight,
                GameRules.ArenaHalfWidth / Mathf.Max(gameCamera.aspect, 0.1f));

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
                return;
            }
            if (!focused) return;
            if (gameOver)
            {
                if (Input.GetKeyDown(KeyCode.R)) RestartGame();
                return;
            }

            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector2 previousPlayer = playerPosition;
            float horizontal = (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f)
                - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);
            float vertical = (Held(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f)
                - (Held(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f);
            GameRules.MovePlayer(ref playerPosition.x, ref playerPosition.y, horizontal, vertical, dt);
            player.position = new Vector3(playerPosition.x, playerPosition.y, 0f);

            fireTimer = Mathf.Max(0f, fireTimer - dt);
            if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
            {
                bullets.Add(CreateActor("Bullet", playerPosition + new Vector2(0f, 0.73f),
                    new Vector3(0.18f, 0.44f, 0.2f), bulletMaterial));
                fireTimer = GameRules.FireInterval;
            }

            MoveActors(enemies, -GameRules.EnemySpeed * dt);
            MoveActors(bullets, GameRules.BulletSpeed * dt);
            ResolveBulletHits();
            foreach (Actor enemy in enemies)
            {
                float hitTime;
                Vector2 start = enemy.Previous - previousPlayer;
                Vector2 end = enemy.Position - playerPosition;
                if (GameRules.SweptOverlap(start.x, start.y, end.x, end.y,
                    GameRules.EnemyHalfWidth + GameRules.PlayerHalfWidth,
                    GameRules.EnemyHalfHeight + GameRules.PlayerHalfHeight, out hitTime))
                {
                    gameOver = true;
                    return;
                }
            }

            RemoveOffscreen(bullets, true);
            RemoveOffscreen(enemies, false);
            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                float x = Random.Range(-GameRules.ArenaHalfWidth + GameRules.EnemyHalfWidth,
                    GameRules.ArenaHalfWidth - GameRules.EnemyHalfWidth);
                enemies.Add(CreateActor("Enemy", new Vector2(x, GameRules.ArenaHalfHeight + 0.6f),
                    new Vector3(1f, 0.8f, 0.4f), enemyMaterial));
                spawnTimer = GameRules.SpawnInterval;
            }
        }

        private static bool Held(KeyCode first, KeyCode second)
        {
            return Input.GetKey(first) || Input.GetKey(second);
        }

        private static void MoveActors(List<Actor> actors, float distance)
        {
            foreach (Actor actor in actors)
            {
                actor.Previous = actor.Position;
                actor.Position.y += distance;
                actor.View.position = new Vector3(actor.Position.x, actor.Position.y, 0f);
            }
        }

        private void ResolveBulletHits()
        {
            for (int b = bullets.Count - 1; b >= 0; b--)
            {
                Actor bullet = bullets[b];
                int nearest = -1;
                float nearestTime = float.PositiveInfinity;
                for (int e = 0; e < enemies.Count; e++)
                {
                    Vector2 start = bullet.Previous - enemies[e].Previous;
                    Vector2 end = bullet.Position - enemies[e].Position;
                    float hitTime;
                    if (GameRules.SweptOverlap(start.x, start.y, end.x, end.y,
                        GameRules.BulletHalfWidth + GameRules.EnemyHalfWidth,
                        GameRules.BulletHalfHeight + GameRules.EnemyHalfHeight, out hitTime)
                        && hitTime < nearestTime)
                    {
                        nearest = e;
                        nearestTime = hitTime;
                    }
                }
                if (nearest < 0) continue;
                RemoveActor(enemies, nearest);
                RemoveActor(bullets, b);
                score += GameRules.PointsPerEnemy;
            }
        }

        private static void RemoveOffscreen(List<Actor> actors, bool upward)
        {
            for (int i = actors.Count - 1; i >= 0; i--)
                if (upward ? actors[i].Position.y > GameRules.ArenaHalfHeight + 1f
                    : actors[i].Position.y < -GameRules.ArenaHalfHeight - 1f)
                    RemoveActor(actors, i);
        }

        private static void RemoveActor(List<Actor> actors, int index)
        {
            // Hide immediately; Unity's Destroy is deferred until the end of this frame.
            actors[index].View.gameObject.SetActive(false);
            Object.Destroy(actors[index].View.gameObject);
            actors.RemoveAt(index);
        }

        private void RestartGame()
        {
            for (int i = enemies.Count - 1; i >= 0; i--) RemoveActor(enemies, i);
            for (int i = bullets.Count - 1; i >= 0; i--) RemoveActor(bullets, i);
            playerPosition = new Vector2(0f, -3.5f);
            player.position = new Vector3(playerPosition.x, playerPosition.y, 0f);
            score = 0;
            spawnTimer = 1f;
            fireTimer = 0f;
            gameOver = false;
        }

        private void OnApplicationFocus(bool hasFocus) { focused = hasFocus; }

        private void OnDestroy()
        {
            foreach (Material material in materials) Object.Destroy(material);
        }

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                    { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                textStyle = new GUIStyle(GUI.skin.label)
                    { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                scoreStyle = new GUIStyle(GUI.skin.label)
                    { fontSize = 24, fontStyle = FontStyle.Bold };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
            }
            Matrix4x4 oldMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 600f);
            float width = Screen.width / scale;
            float height = Screen.height / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            GUI.Label(new Rect(20f, 12f, 300f, 40f), "SCORE  " + score, scoreStyle);
            GUI.Label(new Rect(0f, height - 40f, width, 30f),
                "WASD / Arrows: move    Space: shoot    Esc: quit", textStyle);
            if (gameOver || !focused)
            {
                float x = (width - 420f) / 2f;
                float y = (height - 230f) / 2f;
                GUI.Box(new Rect(x, y, 420f, 230f), GUIContent.none);
                GUI.Label(new Rect(x, y + 20f, 420f, 50f), gameOver ? "GAME OVER" : "PAUSED", titleStyle);
                GUI.Label(new Rect(x, y + 80f, 420f, 35f),
                    gameOver ? "Final score: " + score : "Return to the game window to continue.", textStyle);
                if (gameOver && GUI.Button(new Rect(x + 70f, y + 145f, 280f, 50f),
                    "Restart (R)", buttonStyle)) RestartGame();
            }
            GUI.matrix = oldMatrix;
        }
    }
}
