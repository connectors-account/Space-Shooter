using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    // All gameplay lives in the XY plane. Simple swept rectangle tests need no physics setup.
    public sealed class SpaceShooter : MonoBehaviour
    {
        [SerializeField] private Shader shapeShader;

        private const float HalfWidth = 8f;
        private const float HalfHeight = 6f;
        private const float PlayerSpeed = 7f;
        private const float BulletSpeed = 14f;
        private const float EnemySpeed = 2.5f;
        private const float FireInterval = 0.18f;
        private const float SpawnInterval = 0.85f;
        private static readonly Vector2 PlayerSize = new Vector2(0.8f, 0.8f);
        private static readonly Vector2 BulletSize = new Vector2(0.16f, 0.5f);
        private static readonly Vector2 EnemySize = new Vector2(0.85f, 0.85f);

        private readonly List<Transform> bullets = new List<Transform>();
        private readonly List<Transform> enemies = new List<Transform>();
        private Transform player;
        private Camera gameCamera;
        private Material playerMaterial;
        private Material bulletMaterial;
        private Material enemyMaterial;
        private float fireTimer;
        private float spawnTimer;
        private int score;
        private bool gameOver;
        private bool focused = true;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;

        private void Awake()
        {
            if (shapeShader == null)
            {
                Debug.LogError("Space Shooter requires its Shape shader. Open Assets/Scenes/Main.unity.", this);
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
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.07f);

            playerMaterial = MakeMaterial(new Color(0.15f, 0.8f, 1f));
            bulletMaterial = MakeMaterial(new Color(1f, 0.9f, 0.2f));
            enemyMaterial = MakeMaterial(new Color(1f, 0.25f, 0.3f));
            player = MakeShape("Player", PlayerSize, playerMaterial, Vector2.zero);
            RestartGame();
        }

        private Material MakeMaterial(Color color)
        {
            return new Material(shapeShader) { color = color };
        }

        private Transform MakeShape(string objectName, Vector2 size, Material material, Vector2 position)
        {
            GameObject shape = GameObject.CreatePrimitive(PrimitiveType.Cube);
            shape.name = objectName;
            shape.transform.SetParent(transform);
            shape.transform.localScale = new Vector3(size.x, size.y, 0.2f);
            shape.transform.position = position;
            // Collisions are handled explicitly below; do not run a second physics simulation.
            Destroy(shape.GetComponent<Collider>());
            shape.GetComponent<Renderer>().sharedMaterial = material;
            return shape.transform;
        }

        private void Update()
        {
            if (gameCamera == null) return;
            // Keep the fixed arena visible even when the desktop window is resized.
            gameCamera.orthographicSize = Mathf.Max(HalfHeight + 0.6f,
                (HalfWidth + 0.6f) / Mathf.Max(gameCamera.aspect, 0.1f));
            if (!focused) return;
            if (gameOver)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) RestartGame();
                return;
            }

            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            Vector2 previousPlayerPosition = player.position;
            MovePlayer(dt);
            fireTimer -= dt;
            if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
            {
                Vector2 muzzle = (Vector2)player.position + Vector2.up * 0.7f;
                bullets.Add(MakeShape("Shot", BulletSize, bulletMaterial, muzzle));
                fireTimer = FireInterval;
            }

            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                float x = Random.Range(-HalfWidth + EnemySize.x, HalfWidth - EnemySize.x);
                enemies.Add(MakeShape("Enemy", EnemySize, enemyMaterial,
                    new Vector2(x, HalfHeight + EnemySize.y)));
                spawnTimer = SpawnInterval;
            }

            // Move enemies first; bullet sweep below uses their previous and current positions.
            foreach (Transform enemy in enemies)
                enemy.position += Vector3.down * (EnemySpeed * dt);

            for (int b = bullets.Count - 1; b >= 0; b--)
            {
                Transform bullet = bullets[b];
                Vector2 oldPosition = bullet.position;
                bullet.position += Vector3.up * (BulletSpeed * dt);
                int hit = -1;
                float nearestY = float.PositiveInfinity;
                for (int e = 0; e < enemies.Count; e++)
                {
                    Vector2 enemyPosition = enemies[e].position;
                    Vector2 oldEnemyPosition = enemyPosition + Vector2.up * (EnemySpeed * dt);
                    // Relative sweep includes both moving objects, preventing tunnelling.
                    bool overlapsX = Mathf.Abs(oldPosition.x - enemyPosition.x) <=
                        (BulletSize.x + EnemySize.x) * 0.5f;
                    float startY = oldPosition.y - oldEnemyPosition.y;
                    float endY = bullet.position.y - enemyPosition.y;
                    float radiusY = (BulletSize.y + EnemySize.y) * 0.5f;
                    if (overlapsX && startY <= radiusY && endY >= -radiusY && enemyPosition.y < nearestY)
                    {
                        hit = e;
                        nearestY = enemyPosition.y;
                    }
                }
                if (hit >= 0)
                {
                    RemoveShape(enemies, hit);
                    RemoveShape(bullets, b);
                    score += 10;
                }
                else if (bullet.position.y > HalfHeight + 2f) RemoveShape(bullets, b);
            }

            foreach (Transform enemy in enemies)
            {
                Vector2 current = enemy.position;
                Vector2 previous = current + Vector2.up * (EnemySpeed * dt);
                if (SweptOverlap(previous - previousPlayerPosition,
                    current - (Vector2)player.position, (PlayerSize + EnemySize) * 0.5f)
                    || current.y < -HalfHeight - EnemySize.y * 0.5f)
                {
                    gameOver = true;
                    break;
                }
            }
        }

        private void MovePlayer(float dt)
        {
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f)
                - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f)
                - (Held(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f);
            Vector2 position = (Vector2)player.position
                + Vector2.ClampMagnitude(new Vector2(x, y), 1f) * (PlayerSpeed * dt);
            position.x = Mathf.Clamp(position.x, -HalfWidth + PlayerSize.x, HalfWidth - PlayerSize.x);
            position.y = Mathf.Clamp(position.y, -HalfHeight + PlayerSize.y, HalfHeight - 1.8f);
            player.position = position;
        }

        private static bool Held(KeyCode first, KeyCode second)
        {
            return Input.GetKey(first) || Input.GetKey(second);
        }

        private static bool SweptOverlap(Vector2 from, Vector2 to, Vector2 halfSize)
        {
            // Segment versus expanded rectangle (slab test), including initial overlap.
            float enter = 0f;
            float exit = 1f;
            for (int axis = 0; axis < 2; axis++)
            {
                float delta = to[axis] - from[axis];
                if (Mathf.Abs(delta) < 0.00001f)
                {
                    if (Mathf.Abs(from[axis]) > halfSize[axis]) return false;
                    continue;
                }
                float a = (-halfSize[axis] - from[axis]) / delta;
                float b = (halfSize[axis] - from[axis]) / delta;
                enter = Mathf.Max(enter, Mathf.Min(a, b));
                exit = Mathf.Min(exit, Mathf.Max(a, b));
                if (enter > exit) return false;
            }
            return true;
        }

        private static void RemoveShape(List<Transform> shapes, int index)
        {
            shapes[index].gameObject.SetActive(false);
            Destroy(shapes[index].gameObject);
            shapes.RemoveAt(index);
        }

        private void RestartGame()
        {
            for (int i = bullets.Count - 1; i >= 0; i--) RemoveShape(bullets, i);
            for (int i = enemies.Count - 1; i >= 0; i--) RemoveShape(enemies, i);
            player.position = new Vector3(0f, -4.5f, 0f);
            score = 0;
            fireTimer = 0f;
            spawnTimer = 1f;
            gameOver = false;
        }

        private void OnApplicationFocus(bool hasFocus) { focused = hasFocus; }

        private void OnGUI()
        {
            if (player == null) return;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 22 };
                titleStyle = new GUIStyle(labelStyle) { fontSize = 36, alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24 };
            }
            Matrix4x4 original = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960f * scale) * 0.5f,
                (Screen.height - 720f * scale) * 0.5f, 0f), Quaternion.identity,
                new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(0f, 0f, 960f, 78f), GUIContent.none);
            GUI.Label(new Rect(20f, 8f, 920f, 32f), "Score: " + score + "    |    WASD / Arrows: move    |    Space: shoot", labelStyle);
            GUI.Label(new Rect(20f, 40f, 920f, 32f), "Shoot the red enemies. Do not collide or let one escape below.", labelStyle);
            if (gameOver)
            {
                GUI.Box(new Rect(230f, 235f, 500f, 240f), GUIContent.none);
                GUI.Label(new Rect(240f, 250f, 480f, 55f), "GAME OVER", titleStyle);
                GUI.Label(new Rect(240f, 308f, 480f, 45f), "Score: " + score, titleStyle);
                if (GUI.Button(new Rect(295f, 385f, 370f, 55f), "Restart (R / Enter)", buttonStyle)) RestartGame();
            }
            else if (!focused)
                GUI.Label(new Rect(100f, 320f, 760f, 80f), "Click the game to resume", titleStyle);
            GUI.matrix = original;
        }

        private void OnDestroy()
        {
            if (playerMaterial != null) Destroy(playerMaterial);
            if (bulletMaterial != null) Destroy(bulletMaterial);
            if (enemyMaterial != null) Destroy(enemyMaterial);
        }
    }
}
