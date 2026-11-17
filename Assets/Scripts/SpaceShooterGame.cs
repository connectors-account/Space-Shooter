using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter
{
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private ShooterSimulation game;
        private Camera gameCamera;
        private Transform player;
        private Material material;
        private Mesh quad, triangle;
        private readonly Dictionary<int, Transform> views = new Dictionary<int, Transform>();
        private readonly HashSet<int> liveIds = new HashSet<int>();
        private readonly List<int> staleIds = new List<int>();
        private float accumulator;
        private bool focused = true;
        private GUIStyle titleStyle, labelStyle, buttonStyle;
        private const float Tick = 1f / 120f;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Shader shader = Resources.Load<Shader>("FlatColor");
            if (shader == null)
            {
                Debug.LogError("Space Shooter: Assets/Resources/FlatColor.shader is missing.");
                enabled = false;
                return;
            }
            material = new Material(shader);
            quad = MakeMesh(false);
            triangle = MakeMesh(true);
            gameCamera = new GameObject("Game Camera", typeof(Camera)).GetComponent<Camera>();
            gameCamera.transform.SetParent(transform, false);
            gameCamera.transform.position = new Vector3(0f, 0f, -10f);
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = ShooterSimulation.HalfHeight;
            gameCamera.nearClipPlane = .1f;
            gameCamera.farClipPlane = 30f;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(.025f, .04f, .08f);
            gameCamera.tag = "MainCamera";
            player = Shape("Player", triangle, new Color(.1f, .85f, 1f), .9f, 1f);
            game = new ShooterSimulation(System.Environment.TickCount);
            SyncViews();
        }

        private void Update()
        {
            if (game == null) return;
            // Keep a fixed 16x12 world at any window size. No camera movement.
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            const float target = 4f / 3f;
            gameCamera.rect = aspect >= target
                ? new Rect((1f - target / aspect) * .5f, 0f, target / aspect, 1f)
                : new Rect(0f, (1f - aspect / target) * .5f, 1f, aspect / target);
            if (!focused) return;
            float x, y;
            bool fire, restart, quit;
            ReadInput(out x, out y, out fire, out restart, out quit);
            if (quit) Application.Quit();
            if (game.GameOver)
            {
                accumulator = 0f;
                if (restart) RestartGame();
                return;
            }
            accumulator += Mathf.Min(Time.deltaTime, .05f);
            while (accumulator >= Tick)
            {
                game.Step(Tick, x, y, fire);
                accumulator -= Tick;
            }
            SyncViews();
        }

        private static void ReadInput(out float x, out float y, out bool fire,
            out bool restart, out bool quit)
        {
            x = y = 0f;
            fire = restart = quit = false;
#if ENABLE_INPUT_SYSTEM
            Keyboard k = Keyboard.current;
            if (k == null) return;
            x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1f : 0f)
                - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1f : 0f);
            y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1f : 0f)
                - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1f : 0f);
            fire = k.spaceKey.isPressed;
            restart = k.rKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame;
            quit = k.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            x = ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1f : 0f)
                - ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? 1f : 0f);
            y = ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f)
                - ((Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? 1f : 0f);
            fire = Input.GetKey(KeyCode.Space);
            restart = Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return);
            quit = Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private void RestartGame()
        {
            foreach (Transform view in views.Values)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            views.Clear();
            game.Reset(System.Environment.TickCount);
            accumulator = 0f;
            SyncViews();
        }
        private void OnApplicationFocus(bool hasFocus)
        { focused = hasFocus; accumulator = 0f; }

        private void SyncViews()
        {
            player.position = new Vector3(game.PlayerX, game.PlayerY, 0f);
            liveIds.Clear();
            foreach (ShooterSimulation.Body enemy in game.Enemies)
                SyncBody(enemy, "Enemy", new Color(1f, .23f, .28f), 1f, .8f);
            foreach (ShooterSimulation.Body bullet in game.Bullets)
                SyncBody(bullet, "Shot", new Color(1f, .9f, .25f), .18f, .48f);
            staleIds.Clear();
            foreach (KeyValuePair<int, Transform> entry in views)
                if (!liveIds.Contains(entry.Key)) staleIds.Add(entry.Key);
            foreach (int id in staleIds)
            {
                views[id].gameObject.SetActive(false);
                Destroy(views[id].gameObject);
                views.Remove(id);
            }
        }
        private void SyncBody(ShooterSimulation.Body body, string label, Color color,
            float width, float height)
        {
            liveIds.Add(body.Id);
            Transform view;
            if (!views.TryGetValue(body.Id, out view))
            {
                view = Shape(label, quad, color, width, height);
                views.Add(body.Id, view);
            }
            view.position = new Vector3(body.X, body.Y, 0f);
        }
        private Transform Shape(string label, Mesh mesh, Color color, float width, float height)
        {
            GameObject item = new GameObject(label, typeof(MeshFilter), typeof(MeshRenderer));
            item.transform.SetParent(transform, false);
            item.transform.localScale = new Vector3(width, height, 1f);
            item.GetComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer renderer = item.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
            return item.transform;
        }
        private static Mesh MakeMesh(bool isTriangle)
        {
            var mesh = new Mesh { name = isTriangle ? "Ship triangle" : "Rectangle" };
            mesh.vertices = isTriangle
                ? new[] { new Vector3(0f, .5f), new Vector3(.5f, -.5f), new Vector3(-.5f, -.5f) }
                : new[] { new Vector3(-.5f, -.5f), new Vector3(-.5f, .5f),
                    new Vector3(.5f, .5f), new Vector3(.5f, -.5f) };
            mesh.triangles = isTriangle ? new[] { 0, 1, 2 } : new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnGUI()
        {
            if (game == null) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 32,
                    alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 18,
                    alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
                titleStyle.normal.textColor = Color.white;
                labelStyle.normal.textColor = Color.white;
            }
            Matrix4x4 previous = GUI.matrix;
            // UI is scaled uniformly and centered, independently of the camera.
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960f * scale) / 2f,
                (Screen.height - 720f * scale) / 2f, 0f), Quaternion.identity, Vector3.one * scale);
            GUI.Box(new Rect(10, 10, 940, 42), GUIContent.none);
            GUI.Label(new Rect(20, 14, 160, 34), "Score: " + game.Score, labelStyle);
            GUI.Label(new Rect(195, 14, 745, 34),
                "WASD / Arrows: move   |   Space: shoot   |   Esc: quit", labelStyle);
            if (game.GameOver)
            {
                GUI.Box(new Rect(240, 235, 480, 250), GUIContent.none);
                GUI.Label(new Rect(250, 250, 460, 48), "GAME OVER", titleStyle);
                GUI.Label(new Rect(250, 303, 460, 30), game.EndReason, labelStyle);
                GUI.Label(new Rect(250, 338, 460, 30), "Final score: " + game.Score, labelStyle);
                if (GUI.Button(new Rect(330, 390, 300, 48), "Restart  [R / Enter]", buttonStyle))
                    RestartGame();
            }
            else if (!focused)
                GUI.Label(new Rect(200, 330, 560, 50), "Click the game to resume", titleStyle);
            GUI.matrix = previous;
        }
        private void OnDestroy()
        {
            if (material != null) Destroy(material);
            if (quad != null) Destroy(quad);
            if (triangle != null) Destroy(triangle);
        }
    }
}
