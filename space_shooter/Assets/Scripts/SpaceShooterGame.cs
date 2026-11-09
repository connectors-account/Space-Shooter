using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter
{
    public sealed class SpaceShooterGame : MonoBehaviour
    {
        private ShooterModel model;
        private Camera gameCamera;
        private Sprite square;
        private Texture2D texture;
        private Transform player;
        private readonly Dictionary<int, Transform> visuals = new Dictionary<int, Transform>();
        private readonly HashSet<int> liveIds = new HashSet<int>();
        private readonly List<int> removedIds = new List<int>();
        private GUIStyle titleStyle, textStyle, buttonStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (SceneManager.GetActiveScene().name != "Main") return;
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
            square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
            gameCamera = new GameObject("Game Camera").AddComponent<Camera>();
            gameCamera.transform.SetParent(transform);
            gameCamera.transform.position = new Vector3(0, 0, -10);
            gameCamera.orthographic = true;
            gameCamera.clearFlags = CameraClearFlags.SolidColor;
            gameCamera.backgroundColor = new Color(0.025f, 0.035f, 0.08f);
            player = MakeShape("Player", new Color(0.1f, 0.85f, 1f), model.Player.Width, model.Player.Height);
            Transform nose = MakeShape("Player Nose", Color.white, 0.2f, 0.22f);
            nose.SetParent(player, false);
            nose.localPosition = new Vector3(0, 0.55f, -0.01f);
            SyncVisuals();
        }

        private void Update()
        {
            // Fit the fixed play area in any window without exposing additional spawn space.
            gameCamera.orthographicSize = Mathf.Max(10.5f, 9f / gameCamera.aspect);
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            if (!Application.isFocused) return;
            if (model.GameOver)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.R)) Restart();
                return;
            }
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0)
                - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0)
                - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
            model.Tick(Time.deltaTime, x, y, Input.GetKey(KeyCode.Space));
            SyncVisuals();
        }

        private static bool Held(KeyCode a, KeyCode b) { return Input.GetKey(a) || Input.GetKey(b); }

        private Transform MakeShape(string label, Color color, float width, float height)
        {
            GameObject shape = new GameObject(label);
            shape.transform.SetParent(transform, false);
            shape.transform.localScale = new Vector3(width, height, 1);
            SpriteRenderer renderer = shape.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            return shape.transform;
        }

        private void SyncVisuals()
        {
            player.position = new Vector3(model.Player.X, model.Player.Y, 0);
            liveIds.Clear();
            foreach (ShooterModel.Body bullet in model.Bullets)
                SyncBody(bullet, new Color(1f, 0.85f, 0.2f), "Bullet");
            foreach (ShooterModel.Body enemy in model.Enemies)
                SyncBody(enemy, new Color(1f, 0.25f, 0.3f), "Enemy");
            removedIds.Clear();
            foreach (KeyValuePair<int, Transform> pair in visuals)
                if (!liveIds.Contains(pair.Key)) removedIds.Add(pair.Key);
            foreach (int id in removedIds)
            {
                // Hide immediately; Destroy itself is deferred until the end of the frame.
                visuals[id].gameObject.SetActive(false);
                Destroy(visuals[id].gameObject);
                visuals.Remove(id);
            }
        }

        private void SyncBody(ShooterModel.Body body, Color color, string label)
        {
            liveIds.Add(body.Id);
            Transform visual;
            if (!visuals.TryGetValue(body.Id, out visual))
            {
                visual = MakeShape(label, color, body.Width, body.Height);
                visuals.Add(body.Id, visual);
            }
            visual.position = new Vector3(body.X, body.Y, 0);
        }

        private void Restart() { model.Restart(); SyncVisuals(); }

        private void OnGUI()
        {
            if (model == null) return;
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                textStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
            }
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 960f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 960 * scale) / 2,
                (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.Label(new Rect(20, 10, 920, 45), "SCORE  " + model.Score, titleStyle);
            GUI.Label(new Rect(10, 676, 940, 32), "WASD / Arrows: Move    Hold Space: Shoot    Esc: Quit", textStyle);
            if (model.GameOver)
            {
                GUI.Box(new Rect(230, 230, 500, 260), GUIContent.none);
                GUI.Label(new Rect(250, 250, 460, 45), "GAME OVER", titleStyle);
                GUI.Label(new Rect(250, 303, 460, 30), model.EndReason, textStyle);
                GUI.Label(new Rect(250, 341, 460, 30), "Final score: " + model.Score, textStyle);
                if (GUI.Button(new Rect(330, 398, 300, 55), "Restart (Enter / R)", buttonStyle)) Restart();
            }
            GUI.matrix = previousMatrix;
        }

        private void OnDestroy()
        {
            if (square != null) Destroy(square);
            if (texture != null) Destroy(texture);
        }
    }
}
