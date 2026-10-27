using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MinimalSpaceShooter
{
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private const float Tick = 1f / 120f;
        private ShooterModel model;
        private Camera gameCamera;
        private Transform player;
        private Material playerMaterial;
        private Material enemyMaterial;
        private Material bulletMaterial;
        private readonly Dictionary<int, Transform> views = new Dictionary<int, Transform>();
        private readonly HashSet<int> liveIds = new HashSet<int>();
        private readonly List<int> staleIds = new List<int>();
        private float accumulator;
        private bool paused;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            // The scene is deliberately empty: no inspector references are required.
            if (SceneManager.GetActiveScene().name == "SpaceShooter" &&
                Object.FindFirstObjectByType<SpaceShooterGame>() == null)
                new GameObject("Space Shooter").AddComponent<SpaceShooterGame>();
        }

        private void Awake()
        {
            Shader shader = Resources.Load<Shader>("FlatColor");
            if (shader == null)
            {
                Debug.LogError("Missing Assets/Resources/FlatColor.shader.");
                enabled = false;
                return;
            }
            playerMaterial = MakeMaterial(shader, new Color(0.15f, 0.85f, 1f));
            enemyMaterial = MakeMaterial(shader, new Color(1f, 0.25f, 0.35f));
            bulletMaterial = MakeMaterial(shader, new Color(1f, 0.88f, 0.25f));
            model = new ShooterModel(System.Environment.TickCount);
            Application.targetFrameRate = 60;

            GameObject cameraObject = new GameObject("Game Camera");
            cameraObject.transform.SetParent(transform, false);
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            gameCamera = cameraObject.AddComponent<Camera>();
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = ShooterModel.HalfHeight;
            gameCamera.nearClipPlane = 0.1f;
            gameCamera.farClipPlane = 30f;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.08f);
            // Intentionally no AudioListener, AudioSource, or audio assets.
            player = new GameObject("Player").transform;
            player.SetParent(transform, false);
            MakeCube("Hull", player, Vector3.zero, new Vector3(0.3f, 0.9f, 0.2f), playerMaterial);
            MakeCube("Left Wing", player, new Vector3(-0.325f, -0.15f, 0f),
                new Vector3(0.35f, 0.3f, 0.2f), playerMaterial);
            MakeCube("Right Wing", player, new Vector3(0.325f, -0.15f, 0f),
                new Vector3(0.35f, 0.3f, 0.2f), playerMaterial);
            FitCamera();
            SyncViews();
        }

        private static Material MakeMaterial(Shader shader, Color color)
        {
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private static Transform MakeCube(string name, Transform parent, Vector3 position,
            Vector3 size, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = size;
            // Collisions use the model's swept rectangles, not Unity physics.
            Collider collider = cube.GetComponent<Collider>();
            collider.enabled = false;
            Object.Destroy(collider);
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube.transform;
        }

        private void Update()
        {
            if (model == null) return;
            FitCamera();
            if (Input.GetKeyDown(KeyCode.R)) Restart();
            if (Input.GetKeyDown(KeyCode.Escape) && !model.GameOver) paused = !paused;
            if (!paused && !model.GameOver)
            {
                float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f)
                    - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);
                float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1f : 0f)
                    - (Held(KeyCode.S, KeyCode.DownArrow) ? 1f : 0f);
                bool shoot = Input.GetKey(KeyCode.Space);
                accumulator += Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                while (accumulator >= Tick && !model.GameOver)
                {
                    model.Step(Tick, x, y, shoot);
                    accumulator -= Tick;
                }
            }
            else accumulator = 0f;
            SyncViews();
        }

        private static bool Held(KeyCode first, KeyCode second)
        {
            return Input.GetKey(first) || Input.GetKey(second);
        }

        private void FitCamera()
        {
            float ratio = (float)Screen.width / Mathf.Max(1, Screen.height);
            const float target = 16f / 9f;
            if (ratio > target)
            {
                float width = target / ratio;
                gameCamera.rect = new Rect((1f - width) * 0.5f, 0f, width, 1f);
            }
            else
            {
                float height = ratio / target;
                gameCamera.rect = new Rect(0f, (1f - height) * 0.5f, 1f, height);
            }
        }

        private void SyncViews()
        {
            player.localPosition = new Vector3(model.PlayerX, model.PlayerY, 0f);
            liveIds.Clear();
            SyncBodies(model.Bullets, "Shot", new Vector3(0.18f, 0.44f, 0.15f), bulletMaterial);
            SyncBodies(model.Enemies, "Enemy", new Vector3(0.9f, 0.9f, 0.2f), enemyMaterial);
            staleIds.Clear();
            foreach (KeyValuePair<int, Transform> pair in views)
                if (!liveIds.Contains(pair.Key)) staleIds.Add(pair.Key);
            foreach (int id in staleIds)
            {
                Destroy(views[id].gameObject);
                views.Remove(id);
            }
        }

        private void SyncBodies(IReadOnlyList<ShooterModel.Body> bodies, string name,
            Vector3 size, Material material)
        {
            for (int i = 0; i < bodies.Count; i++)
            {
                ShooterModel.Body body = bodies[i];
                liveIds.Add(body.Id);
                Transform view;
                if (!views.TryGetValue(body.Id, out view))
                {
                    view = MakeCube(name, transform, Vector3.zero, size, material);
                    views.Add(body.Id, view);
                }
                view.localPosition = new Vector3(body.X, body.Y, 0f);
            }
        }

        private void Restart()
        {
            foreach (Transform view in views.Values)
            {
                view.gameObject.SetActive(false);
                Destroy(view.gameObject);
            }
            views.Clear();
            model.Restart();
            paused = false;
            accumulator = 0f;
            SyncViews();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) { paused = true; accumulator = 0f; }
        }

        private void OnGUI()
        {
            if (model == null) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 38,
                    fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 19,
                    alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 21 };
            }
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 540f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960f * scale) / 2f,
                (Screen.height - 540f * scale) / 2f, 0f), Quaternion.identity,
                new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(0, 0, 960, 43), GUIContent.none);
            GUI.Label(new Rect(15, 3, 150, 36), "SCORE  " + model.Score, labelStyle);
            GUI.Label(new Rect(180, 3, 765, 36), "WASD / arrows: move   |   Space: fire   |   Esc: pause   |   R: restart", labelStyle);
            if (paused || model.GameOver)
            {
                GUI.Box(new Rect(225, 145, 510, 255), GUIContent.none);
                GUI.Label(new Rect(240, 158, 480, 62), model.GameOver ? "GAME OVER" : "PAUSED", titleStyle);
                GUI.Label(new Rect(240, 222, 480, 32), "Score: " + model.Score, labelStyle);
                GUI.Label(new Rect(240, 258, 480, 32), model.GameOver
                    ? "A ship hit you or escaped past the bottom."
                    : "Destroy red ships before they escape.", labelStyle);
                if (GUI.Button(new Rect(315, 315, 330, 56), model.GameOver
                    ? "Restart (R)" : "Resume (Esc)", buttonStyle))
                {
                    if (model.GameOver) Restart();
                    else paused = false;
                }
            }
            GUI.matrix = previous;
        }

        private void OnDestroy()
        {
            if (playerMaterial != null) Destroy(playerMaterial);
            if (enemyMaterial != null) Destroy(enemyMaterial);
            if (bulletMaterial != null) Destroy(bulletMaterial);
        }
    }
}
