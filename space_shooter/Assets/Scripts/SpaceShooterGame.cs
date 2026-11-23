using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    // One runtime-created controller; no inspector wiring or prefab assets are required.
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private const float HalfHeight = 5f;
        private const float HalfWidth = HalfHeight * 16f / 9f;
        private const float PlayerSpeed = 6f;
        private const float ShotSpeed = 14f;
        private const float EnemySpeed = 2.4f;
        private const float ShotInterval = 0.18f;
        private const float SpawnInterval = 0.85f;
        private static readonly Vector2 PlayerSize = new Vector2(0.7f, 0.7f);
        private static readonly Vector2 ShotSize = new Vector2(0.16f, 0.42f);
        private static readonly Vector2 EnemySize = new Vector2(0.8f, 0.7f);

        private sealed class Body
        {
            public Transform View;
            public Vector2 Position;
            public Vector2 Previous;
            public Vector2 Size;
        }

        private readonly List<Body> shots = new List<Body>();
        private readonly List<Body> enemies = new List<Body>();
        private Sprite square;
        private Camera gameCamera;
        private Body player;
        private Vector2 movement;
        private bool shooting;
        private bool gameOver;
        private bool restartRequested;
        private float shotTimer;
        private float spawnTimer;
        private int score;
        private GUIStyle hudStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            new GameObject("Space Shooter").AddComponent<SpaceShooterGame>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 0.02f;
            // A single white rectangle tinted by SpriteRenderer; no image assets or audio.
            square = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1),
                new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
            square.name = "Built-in Square";
            var cameraObject = new GameObject("Game Camera");
            cameraObject.transform.SetParent(transform);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            gameCamera = cameraObject.AddComponent<Camera>();
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = HalfHeight;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.075f);
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 30f;
            RestartGame();
        }

        private Body CreateBody(string label, Vector2 position, Vector2 size, Color color)
        {
            var view = new GameObject(label);
            view.transform.SetParent(transform);
            view.transform.position = position;
            view.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = view.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            return new Body { View = view.transform, Position = position, Previous = position, Size = size };
        }

        private static void RemoveBody(Body body)
        {
            // Hide immediately: Destroy is deferred until the end of the frame.
            body.View.gameObject.SetActive(false);
            Destroy(body.View.gameObject);
        }

        private void RestartGame()
        {
            foreach (Body shot in shots) RemoveBody(shot);
            foreach (Body enemy in enemies) RemoveBody(enemy);
            shots.Clear();
            enemies.Clear();
            if (player != null) RemoveBody(player);
            player = CreateBody("Player", new Vector2(0f, -3.7f), PlayerSize, Color.cyan);
            score = 0;
            shotTimer = 0f;
            spawnTimer = 1f;
            movement = Vector2.zero;
            shooting = false;
            gameOver = false;
            restartRequested = false;
        }

        private void Update()
        {
            // Keep the same play area at any window size; letterbox the camera.
            float aspect = Screen.width / (float)Mathf.Max(1, Screen.height);
            const float targetAspect = 16f / 9f;
            gameCamera.rect = aspect > targetAspect
                ? new Rect((1f - targetAspect / aspect) * 0.5f, 0f, targetAspect / aspect, 1f)
                : new Rect(0f, (1f - aspect / targetAspect) * 0.5f, 1f, aspect / targetAspect);

            if (gameOver)
            {
                if (restartRequested || Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return))
                    RestartGame();
                return;
            }

            float horizontal = (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f)
                - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);
            float vertical = (Held(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f)
                - (Held(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f);
            movement = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
            shooting = Input.GetKey(KeyCode.Space);
        }

        private static bool Held(KeyCode first, KeyCode second)
        {
            return Input.GetKey(first) || Input.GetKey(second);
        }

        private void FixedUpdate()
        {
            if (gameOver) return;
            float dt = Time.fixedDeltaTime;
            player.Previous = player.Position;
            player.Position += movement * (PlayerSpeed * dt);
            player.Position = new Vector2(
                Mathf.Clamp(player.Position.x, -HalfWidth + PlayerSize.x / 2f, HalfWidth - PlayerSize.x / 2f),
                Mathf.Clamp(player.Position.y, -HalfHeight + PlayerSize.y / 2f, HalfHeight - PlayerSize.y / 2f));
            player.View.position = player.Position;

            shotTimer -= dt;
            if (shooting && shotTimer <= 0f)
            {
                shots.Add(CreateBody("Shot", player.Position + Vector2.up * 0.6f,
                    ShotSize, new Color(1f, 0.9f, 0.25f)));
                shotTimer = ShotInterval;
            }

            spawnTimer -= dt;
            if (spawnTimer <= 0f)
            {
                float x = Random.Range(-HalfWidth + EnemySize.x, HalfWidth - EnemySize.x);
                enemies.Add(CreateBody("Enemy", new Vector2(x, HalfHeight + EnemySize.y),
                    EnemySize, new Color(1f, 0.25f, 0.3f)));
                spawnTimer = SpawnInterval;
            }

            foreach (Body enemy in enemies) Move(enemy, Vector2.down * (EnemySpeed * dt));
            foreach (Body shot in shots) Move(shot, Vector2.up * (ShotSpeed * dt));

            // Relative-motion swept boxes prevent fast bullets from passing through enemies.
            for (int s = shots.Count - 1; s >= 0; s--)
            {
                for (int e = enemies.Count - 1; e >= 0; e--)
                {
                    if (!SweptOverlap(shots[s], enemies[e])) continue;
                    RemoveBody(enemies[e]);
                    enemies.RemoveAt(e);
                    RemoveBody(shots[s]);
                    shots.RemoveAt(s);
                    score += 10;
                    break;
                }
            }

            foreach (Body enemy in enemies)
            {
                if (SweptOverlap(player, enemy) || enemy.Position.y + enemy.Size.y / 2f < -HalfHeight)
                {
                    gameOver = true;
                    movement = Vector2.zero;
                    shooting = false;
                    break;
                }
            }

            for (int s = shots.Count - 1; s >= 0; s--)
            {
                if (shots[s].Position.y - shots[s].Size.y / 2f <= HalfHeight) continue;
                RemoveBody(shots[s]);
                shots.RemoveAt(s);
            }
        }

        private static void Move(Body body, Vector2 delta)
        {
            body.Previous = body.Position;
            body.Position += delta;
            body.View.position = body.Position;
        }

        private static bool SweptOverlap(Body a, Body b)
        {
            Vector2 start = a.Previous - b.Previous;
            Vector2 delta = (a.Position - b.Position) - start;
            Vector2 extent = (a.Size + b.Size) * 0.5f;
            float entry = 0f;
            float exit = 1f;
            return ClipAxis(start.x, delta.x, extent.x, ref entry, ref exit)
                && ClipAxis(start.y, delta.y, extent.y, ref entry, ref exit);
        }

        private static bool ClipAxis(float start, float delta, float extent, ref float entry, ref float exit)
        {
            if (Mathf.Abs(delta) < 0.000001f) return Mathf.Abs(start) <= extent;
            float first = (-extent - start) / delta;
            float last = (extent - start) / delta;
            entry = Mathf.Max(entry, Mathf.Min(first, last));
            exit = Mathf.Min(exit, Mathf.Max(first, last));
            return entry <= exit;
        }

        private void OnApplicationFocus(bool focused)
        {
            if (focused) return;
            movement = Vector2.zero;
            shooting = false;
        }

        private void OnGUI()
        {
            if (hudStyle == null)
            {
                hudStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
                titleStyle = new GUIStyle(hudStyle) { fontSize = 46, alignment = TextAnchor.MiddleCenter };
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 24 };
                hudStyle.normal.textColor = Color.white;
                titleStyle.normal.textColor = Color.white;
                bodyStyle.normal.textColor = Color.white;
            }

            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            Rect viewport = gameCamera.pixelRect;
            // Paint letterbox bars explicitly; pixels outside Camera.rect are not cleared by it.
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height - viewport.yMax), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height - viewport.yMin, Screen.width, viewport.yMin), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, 0, viewport.xMin, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(viewport.xMax, 0, Screen.width - viewport.xMax, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.x, Screen.height - viewport.yMax, 0f),
                Quaternion.identity, Vector3.one * (viewport.width / 1280f));
            GUI.Label(new Rect(24, 16, 400, 44), "SCORE  " + score, hudStyle);
            GUI.Label(new Rect(480, 20, 780, 32), "WASD / Arrows: move     Space: shoot", bodyStyle);

            if (gameOver)
            {
                GUI.color = new Color(0f, 0f, 0f, 0.85f);
                GUI.DrawTexture(new Rect(310, 205, 660, 310), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(330, 225, 620, 70), "GAME OVER", titleStyle);
                GUI.Label(new Rect(330, 305, 620, 40), "Final score: " + score, bodyStyle);
                GUI.Label(new Rect(330, 350, 620, 40), "Avoid enemies. Don't let them pass you.", bodyStyle);
                if (GUI.Button(new Rect(450, 420, 380, 60), "Restart  (R / Enter)", buttonStyle))
                    restartRequested = true;
            }
            GUI.matrix = previousMatrix;
            GUI.color = previousColor;
        }

        private void OnDestroy()
        {
            if (square != null) Destroy(square);
        }
    }
}
