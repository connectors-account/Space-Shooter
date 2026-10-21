using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private sealed class Body
        {
            public GameObject Object;
            public Vector2 Position;
            public Vector2 Previous;
        }

        private readonly List<Body> enemies = new List<Body>();
        private readonly List<Body> bullets = new List<Body>();
        private readonly List<Material> materials = new List<Material>();
        private Camera gameCamera;
        private Body player;
        private Material playerMaterial, enemyMaterial, bulletMaterial;
        private float spawnTimer, shotTimer;
        private int score;
        private bool gameOver;
        private GUIStyle labelStyle, titleStyle, buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (FindObjectOfType<SpaceShooterGame>() == null)
                new GameObject("Space Shooter").AddComponent<SpaceShooterGame>();
        }

        private void Awake()
        {
            gameCamera = new GameObject("Game Camera").AddComponent<Camera>();
            gameCamera.transform.SetParent(transform);
            gameCamera.transform.position = new Vector3(0f, 0f, -20f);
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = GameRules.HalfHeight;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.075f);
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 40f;
            Shader shader = Resources.Load<Shader>("ShipColor");
            if (shader == null)
            {
                Debug.LogError("Missing Assets/Resources/ShipColor.shader.");
                enabled = false;
                return;
            }
            playerMaterial = MakeMaterial(shader, new Color(0.15f, 0.85f, 1f));
            enemyMaterial = MakeMaterial(shader, new Color(1f, 0.25f, 0.35f));
            bulletMaterial = MakeMaterial(shader, new Color(1f, 0.9f, 0.3f));
            Restart();
        }

        private Material MakeMaterial(Shader shader, Color color)
        {
            Material material = new Material(shader);
            material.color = color;
            materials.Add(material);
            return material;
        }

        private GameObject Cube(string name, Transform parent, Vector3 position,
            Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = scale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            // All collisions use swept boxes below; no physics simulation is needed.
            Collider collider = cube.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            return cube;
        }

        private Body CreateBody(string name, Vector2 position, Material material,
            Vector3 scale)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(transform);
            root.transform.position = position;
            Cube("Body", root.transform, Vector3.zero, scale, material);
            return new Body { Object = root, Position = position, Previous = position };
        }

        private void Restart()
        {
            Clear(enemies);
            Clear(bullets);
            if (player != null) Remove(player);
            player = CreateBody("Player", new Vector2(0f, -4.5f), playerMaterial,
                new Vector3(0.35f, 0.9f, 0.3f));
            Cube("Wings", player.Object.transform, new Vector3(0f, -0.15f, 0f),
                new Vector3(1.1f, 0.35f, 0.3f), playerMaterial);
            score = 0;
            spawnTimer = 1.2f;
            shotTimer = 0f;
            gameOver = false;
        }

        private void Update()
        {
            // Keep the same play field on every window size, using letterboxing.
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            const float targetAspect = 4f / 3f;
            gameCamera.rect = aspect >= targetAspect
                ? new Rect((1f - targetAspect / aspect) * 0.5f, 0f, targetAspect / aspect, 1f)
                : new Rect(0f, (1f - aspect / targetAspect) * 0.5f, 1f, aspect / targetAspect);
            if (gameOver || !Application.isFocused) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            player.Previous = player.Position;
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f)
                - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f)
                - (Held(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f);
            Vector2 movement = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            player.Position += movement * (6f * dt);
            float limitX = GameRules.HalfWidth - GameRules.PlayerHalfWidth;
            float limitY = GameRules.HalfHeight - GameRules.PlayerHalfHeight;
            player.Position.x = GameRules.Clamp(player.Position.x, -limitX, limitX);
            player.Position.y = GameRules.Clamp(player.Position.y, -limitY, limitY);
            Sync(player);

            shotTimer -= dt;
            if (Input.GetKey(KeyCode.Space) && shotTimer <= 0f)
            {
                bullets.Add(CreateBody("Shot", player.Position + Vector2.up * 0.7f,
                    bulletMaterial, new Vector3(0.2f, 0.5f, 0.2f)));
                shotTimer = 0.2f;
            }
            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                enemies.Add(CreateBody("Enemy", new Vector2(Random.Range(-7.3f, 7.3f), 6.5f),
                    enemyMaterial, new Vector3(0.9f, 0.9f, 0.3f)));
                spawnTimer = 0.85f;
            }
            foreach (Body enemy in enemies) Move(enemy, Vector2.down * (2.2f * dt));
            foreach (Body bullet in bullets) Move(bullet, Vector2.up * (12f * dt));

            // Each shot destroys at most one enemy: the first along its trajectory.
            for (int b = bullets.Count - 1; b >= 0; b--)
            {
                Body bullet = bullets[b];
                int first = -1;
                float earliest = float.PositiveInfinity;
                for (int e = 0; e < enemies.Count; e++)
                {
                    float time;
                    if (Hit(bullet, enemies[e], GameRules.BulletHalfWidth + GameRules.EnemyHalfSize,
                        GameRules.BulletHalfHeight + GameRules.EnemyHalfSize, out time) && time < earliest)
                    { first = e; earliest = time; }
                }
                if (first >= 0)
                {
                    Remove(enemies[first]);
                    enemies.RemoveAt(first);
                    Remove(bullet);
                    bullets.RemoveAt(b);
                    score += 10;
                }
                else if (bullet.Position.y > 6.75f)
                { Remove(bullet); bullets.RemoveAt(b); }
            }
            foreach (Body enemy in enemies)
            {
                float time;
                if (enemy.Position.y < -6.45f || Hit(player, enemy,
                    GameRules.PlayerHalfWidth + GameRules.EnemyHalfSize,
                    GameRules.PlayerHalfHeight + GameRules.EnemyHalfSize, out time))
                { gameOver = true; break; }
            }
        }

        private static bool Held(KeyCode a, KeyCode b) { return Input.GetKey(a) || Input.GetKey(b); }
        private static void Sync(Body body) { body.Object.transform.position = body.Position; }
        private static void Move(Body body, Vector2 delta)
        {
            body.Previous = body.Position;
            body.Position += delta;
            Sync(body);
        }
        private static bool Hit(Body a, Body b, float width, float height, out float time)
        {
            Vector2 start = a.Previous - b.Previous;
            Vector2 end = a.Position - b.Position;
            return GameRules.SweptHit(start.x, start.y, end.x, end.y, width, height, out time);
        }
        private static void Remove(Body body)
        {
            body.Object.SetActive(false);
            Destroy(body.Object);
        }
        private static void Clear(List<Body> bodies)
        {
            foreach (Body body in bodies) Remove(body);
            bodies.Clear();
        }

        private void OnGUI()
        {
            if (gameCamera == null || player == null) return;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 23 };
                labelStyle.normal.textColor = Color.white;
                titleStyle = new GUIStyle(labelStyle) { fontSize = 42, alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 26 };
            }
            Rect viewport = gameCamera.pixelRect;
            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.x, Screen.height - viewport.yMax, 0f),
                Quaternion.identity, new Vector3(viewport.width / 960f, viewport.height / 720f, 1f));
            GUI.Box(new Rect(12f, 12f, 936f, 74f), GUIContent.none);
            GUI.Label(new Rect(28f, 20f, 900f, 32f), "SCORE  " + score, labelStyle);
            GUI.Label(new Rect(28f, 51f, 900f, 30f), "WASD / Arrows: move   |   Space: shoot   |   Stop enemies escaping", labelStyle);
            if (gameOver)
            {
                GUI.Box(new Rect(240f, 220f, 480f, 280f), GUIContent.none);
                GUI.Label(new Rect(250f, 240f, 460f, 65f), "GAME OVER", titleStyle);
                GUI.Label(new Rect(250f, 310f, 460f, 65f), "Score: " + score, titleStyle);
                if (GUI.Button(new Rect(330f, 405f, 300f, 65f), "Restart", buttonStyle)) Restart();
            }
            GUI.matrix = oldMatrix;
        }

        private void OnDestroy()
        {
            foreach (Material material in materials) Destroy(material);
        }
    }
}
