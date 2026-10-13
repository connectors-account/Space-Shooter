using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private ShooterModel model;
        private Camera gameCamera;
        private Transform player;
        private Sprite square;
        private Texture2D texture;
        private readonly List<Transform> bulletViews = new List<Transform>();
        private readonly List<Transform> enemyViews = new List<Transform>();
        private bool paused;
        private GUIStyle titleStyle, labelStyle, buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindFirstObjectByType<SpaceShooterGame>() == null)
                new GameObject("Space Shooter").AddComponent<SpaceShooterGame>();
        }

        private void Awake()
        {
            model = new ShooterModel(System.Environment.TickCount);
            Application.targetFrameRate = 60;
            texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            texture.filterMode = FilterMode.Point;
            square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            gameCamera = new GameObject("Game Camera").AddComponent<Camera>();
            gameCamera.transform.SetParent(transform);
            gameCamera.transform.position = new Vector3(0, 0, -10);
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = 5;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.08f);
            gameCamera.tag = "MainCamera";
            player = CreateShip("Player", new Color(0.15f, 0.85f, 1f), false);
            SyncViews();
            ResizeCamera();
        }

        private Transform Rectangle(string objectName, Transform parent, Vector2 size, Color color)
        {
            var item = new GameObject(objectName);
            item.transform.SetParent(parent, false);
            item.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            return item.transform;
        }

        private Transform CreateShip(string objectName, Color color, bool enemy)
        {
            var root = new GameObject(objectName).transform;
            root.SetParent(transform, false);
            Rectangle("Body", root, new Vector2(0.32f, 0.8f), color);
            var wings = Rectangle("Wings", root, new Vector2(0.8f, 0.24f), color);
            wings.localPosition = new Vector3(0, enemy ? 0.15f : -0.15f, 0);
            return root;
        }

        private void Update()
        {
            ResizeCamera();
            if (model.GameOver)
            {
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) Restart();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape)) paused = !paused;
            if (paused) return;
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) -
                      (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) -
                      (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
            model.Advance(Time.deltaTime, x, y, Input.GetKey(KeyCode.Space));
            SyncViews();
        }

        private static bool Held(KeyCode a, KeyCode b) { return Input.GetKey(a) || Input.GetKey(b); }

        private void ResizeCamera()
        {
            // Expand the view for unusual window sizes; game bounds never change.
            float aspect = (float)Screen.width / Mathf.Max(1, Screen.height);
            gameCamera.orthographicSize = Mathf.Max(5f, 8f / aspect);
        }

        private void SyncViews()
        {
            player.position = new Vector3(model.PlayerX, model.PlayerY, 0);
            SyncBodies(model.Bullets, bulletViews, false);
            SyncBodies(model.Enemies, enemyViews, true);
        }

        private void SyncBodies(List<ShooterModel.Body> bodies, List<Transform> views, bool enemies)
        {
            while (views.Count > bodies.Count)
            {
                int index = views.Count - 1;
                views[index].gameObject.SetActive(false);
                Destroy(views[index].gameObject);
                views.RemoveAt(index);
            }
            while (views.Count < bodies.Count)
            {
                views.Add(enemies
                    ? CreateShip("Enemy", new Color(1f, 0.25f, 0.35f), true)
                    : Rectangle("Bullet", transform, new Vector2(0.16f, 0.36f), new Color(1f, 0.9f, 0.2f)));
            }
            for (int i = 0; i < bodies.Count; i++)
                views[i].position = new Vector3(bodies[i].X, bodies[i].Y, 0);
        }

        private void Restart()
        {
            model.Restart();
            paused = false;
            SyncViews();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && model != null && !model.GameOver) paused = true;
        }

        private void OnGUI()
        {
            if (model == null) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 34, alignment = TextAnchor.MiddleCenter };
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
            }
            // A fixed logical UI size keeps text usable on high-DPI desktop displays.
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
                new Vector3(Screen.width / 960f, Screen.height / 600f, 1));
            GUI.Box(new Rect(12, 10, 936, 38), GUIContent.none);
            GUI.Label(new Rect(18, 10, 924, 38), "SCORE  " + model.Score + "     |     Destroy enemies before they escape", labelStyle);
            GUI.Label(new Rect(12, 562, 936, 28), "WASD / arrows: move    •    Space: fire    •    Esc: pause", labelStyle);
            if (model.GameOver || paused)
            {
                GUI.Box(new Rect(230, 165, 500, 275), GUIContent.none);
                GUI.Label(new Rect(250, 185, 460, 55), model.GameOver ? "GAME OVER" : "PAUSED", titleStyle);
                GUI.Label(new Rect(250, 250, 460, 45), model.GameOver ? "Final score: " + model.Score : "Press Esc or Resume to continue", labelStyle);
                if (GUI.Button(new Rect(330, 310, 300, 48), model.GameOver ? "Restart (R / Enter)" : "Resume", buttonStyle))
                {
                    if (model.GameOver) Restart(); else paused = false;
                }
                if (GUI.Button(new Rect(330, 370, 300, 42), "Quit", buttonStyle))
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                }
            }
            GUI.matrix = previous;
        }

        private void OnDestroy()
        {
            if (square != null) Destroy(square);
            if (texture != null) Destroy(texture);
        }
    }
}
