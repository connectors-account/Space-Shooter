using System.Collections.Generic;
using UnityEngine;

// A complete small game: logical coordinates are 960 x 720, with Y pointing down.
// IMGUI draws built-in rectangles; no sprites, physics, UI packages or audio needed.
public sealed class SpaceShooter : MonoBehaviour
{
    private const float Width = 960f;
    private const float Height = 720f;
    private const float PlayerSpeed = 360f;
    private const float ShotSpeed = 640f;
    private const float EnemySpeed = 180f;
    private const float ShotInterval = 0.18f;
    private const float SpawnInterval = 0.8f;

    private readonly List<Rect> shots = new List<Rect>();
    private readonly List<Rect> enemies = new List<Rect>();
    private Rect player;
    private float shotTimer;
    private float spawnTimer;
    private int score;
    private bool gameOver;
    private GUIStyle titleStyle;
    private GUIStyle textStyle;
    private GUIStyle buttonStyle;

    private void Awake()
    {
        // The supplied scene needs only this component. Create the clearing camera.
        Camera camera = new GameObject("Background Camera").AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.015f, 0.02f, 0.04f);
        camera.cullingMask = 0;
        camera.orthographic = true;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Restart();
    }

    private void Restart()
    {
        shots.Clear();
        enemies.Clear();
        player = new Rect(Width / 2f - 21f, Height - 90f, 42f, 36f);
        shotTimer = 0f;
        spawnTimer = 1f;
        score = 0;
        gameOver = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Return)) Restart();
            return;
        }
        if (!Application.isFocused) return;

        // Clamp stalls so objects cannot leap through each other after a long frame.
        float dt = Mathf.Min(Time.deltaTime, 1f / 30f);
        Vector2 move = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move.y -= 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move.y += 1f;
        if (move.sqrMagnitude > 1f) move.Normalize();
        player.position += move * (PlayerSpeed * dt);
        player.x = Mathf.Clamp(player.x, 16f, Width - player.width - 16f);
        player.y = Mathf.Clamp(player.y, 84f, Height - player.height - 16f);

        shotTimer -= dt;
        if (Input.GetKey(KeyCode.Space) && shotTimer <= 0f)
        {
            shots.Add(new Rect(player.center.x - 3f, player.y - 14f, 6f, 14f));
            shotTimer = ShotInterval;
        }
        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            enemies.Add(new Rect(Random.Range(16f, Width - 58f), 68f, 42f, 32f));
            spawnTimer = SpawnInterval;
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            Rect enemy = enemies[i];
            enemy.y += EnemySpeed * dt;
            enemies[i] = enemy;
        }

        // Swept vertical shot rectangles cover the whole distance travelled this frame.
        // Reverse iteration permits removal without skipping the next shot or enemy.
        for (int i = shots.Count - 1; i >= 0; i--)
        {
            Rect shot = shots[i];
            float oldBottom = shot.yMax;
            shot.y -= ShotSpeed * dt;
            shots[i] = shot;
            Rect sweep = new Rect(shot.x, shot.y, shot.width, oldBottom - shot.y);
            int hit = -1;
            float nearestY = float.NegativeInfinity;
            for (int j = 0; j < enemies.Count; j++)
            {
                // Include enemy movement too; choose the first enemy along the shot path.
                Rect enemySweep = enemies[j];
                enemySweep.yMin -= EnemySpeed * dt;
                if (sweep.Overlaps(enemySweep) && enemies[j].y > nearestY)
                {
                    hit = j;
                    nearestY = enemies[j].y;
                }
            }
            if (hit >= 0)
            {
                enemies.RemoveAt(hit);
                shots.RemoveAt(i);
                score += 10;
            }
            else if (shot.yMax < 68f) shots.RemoveAt(i);
        }

        foreach (Rect enemy in enemies)
        {
            if (enemy.Overlaps(player) || enemy.yMax >= Height)
            {
                gameOver = true;
                break;
            }
        }
    }

    private static void Fill(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void OnGUI()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 32, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            textStyle = new GUIStyle(GUI.skin.label)
                { fontSize = 18, alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
            titleStyle.normal.textColor = Color.white;
            textStyle.normal.textColor = Color.white;
        }

        // Letterbox instead of stretching: simulation bounds never depend on resolution.
        Matrix4x4 savedMatrix = GUI.matrix;
        Color savedColor = GUI.color;
        float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
        Vector3 offset = new Vector3((Screen.width - Width * scale) / 2f,
                                    (Screen.height - Height * scale) / 2f, 0f);
        GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1f));
        Fill(new Rect(0f, 0f, Width, Height), new Color(0.03f, 0.05f, 0.10f));

        Color cyan = new Color(0.20f, 0.88f, 1f);
        Fill(player, cyan);
        Fill(new Rect(player.x + 13f, player.y + 5f, 16f, 12f), new Color(0.04f, 0.15f, 0.25f));
        foreach (Rect shot in shots) Fill(shot, new Color(1f, 0.90f, 0.35f));
        foreach (Rect enemy in enemies)
        {
            Fill(enemy, new Color(1f, 0.27f, 0.32f));
            Fill(new Rect(enemy.x + 8f, enemy.y + 7f, 26f, 6f), new Color(0.32f, 0.04f, 0.09f));
        }

        Fill(new Rect(0f, 0f, Width, 68f), new Color(0.02f, 0.03f, 0.06f));
        GUI.Label(new Rect(16f, 10f, 200f, 45f), "SCORE  " + score, titleStyle);
        GUI.Label(new Rect(235f, 8f, 710f, 28f), "WASD / arrows: move    Space: shoot    Esc: quit", textStyle);
        GUI.Label(new Rect(235f, 34f, 710f, 26f), "Destroy enemies. Don't collide or let one reach the bottom.", textStyle);

        if (gameOver)
        {
            Fill(new Rect(0f, 68f, Width, Height - 68f), new Color(0f, 0f, 0f, 0.82f));
            GUI.Label(new Rect(280f, 240f, 400f, 65f), "GAME OVER", titleStyle);
            GUI.Label(new Rect(280f, 310f, 400f, 40f), "Final score: " + score, textStyle);
            if (GUI.Button(new Rect(350f, 375f, 260f, 55f), "Restart", buttonStyle)) Restart();
            GUI.Label(new Rect(280f, 440f, 400f, 35f), "or press R / Enter", textStyle);
        }
        GUI.matrix = savedMatrix;
        GUI.color = savedColor;
    }
}
