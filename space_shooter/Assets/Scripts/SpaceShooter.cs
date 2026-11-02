using System.Collections.Generic;
using UnityEngine;

// One controller, immediate-mode coloured shapes, and rectangle collisions.
public sealed class SpaceShooter : MonoBehaviour
{
    private const float Width = 960f, Height = 600f;
    private const float PlayerSpeed = 360f, BulletSpeed = 600f, EnemySpeed = 150f;
    private readonly List<Rect> bullets = new List<Rect>();
    private readonly List<Rect> enemies = new List<Rect>();
    private Rect player;
    private float fireTimer, spawnTimer;
    private int score;
    private bool gameOver;
    private GUIStyle labelStyle, titleStyle, buttonStyle;

    // Also works when Play is pressed in a new, empty scene.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<SpaceShooter>() == null)
            new GameObject("Space Shooter").AddComponent<SpaceShooter>();
    }

    private void Awake()
    {
        Camera camera = new GameObject("Background Camera").AddComponent<Camera>();
        camera.transform.SetParent(transform);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.cullingMask = 0; // All shapes and text are drawn by OnGUI.
        camera.depth = 100;
        ResetGame();
    }

    private void ResetGame()
    {
        player = new Rect(Width / 2f - 18f, Height - 65f, 36f, 30f);
        bullets.Clear();
        enemies.Clear();
        fireTimer = 0f;
        spawnTimer = 0.8f;
        score = 0;
        gameOver = false;
    }

    private void Update()
    {
        if (gameOver)
        {
            if (Input.GetKeyDown(KeyCode.R)) ResetGame();
            return;
        }
        if (!Application.isFocused) return;
        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        Vector2 direction = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction.x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) direction.y -= 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) direction.y += 1f;
        direction = Vector2.ClampMagnitude(direction, 1f);
        player.position += direction * PlayerSpeed * dt;
        player.x = Mathf.Clamp(player.x, 0f, Width - player.width);
        player.y = Mathf.Clamp(player.y, 48f, Height - player.height);

        fireTimer -= dt;
        if (Input.GetKey(KeyCode.Space) && fireTimer <= 0f)
        {
            bullets.Add(new Rect(player.center.x - 3f, player.y - 14f, 6f, 14f));
            fireTimer = 0.18f;
        }
        spawnTimer -= dt;
        if (spawnTimer <= 0f)
        {
            enemies.Add(new Rect(Random.Range(0f, Width - 34f), -34f, 34f, 34f));
            spawnTimer = 0.8f;
        }

        // Move enemies first; bullets sweep their relative vertical motion below.
        for (int i = 0; i < enemies.Count; i++)
        {
            Rect enemy = enemies[i];
            enemy.y += EnemySpeed * dt;
            enemies[i] = enemy;
        }
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            Rect bullet = bullets[i];
            bullet.y -= BulletSpeed * dt;
            bullets[i] = bullet;
            Rect sweep = new Rect(bullet.x, bullet.y, bullet.width,
                bullet.height + (BulletSpeed + EnemySpeed) * dt);
            int hit = -1;
            float nearestY = float.NegativeInfinity;
            for (int j = 0; j < enemies.Count; j++)
            {
                if (sweep.Overlaps(enemies[j]) && enemies[j].y > nearestY)
                {
                    hit = j;
                    nearestY = enemies[j].y;
                }
            }
            if (hit >= 0)
            {
                enemies.RemoveAt(hit);
                bullets.RemoveAt(i);
                score += 10;
            }
            else if (bullet.yMax < 0f) bullets.RemoveAt(i);
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

    private static void DrawBox(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 20 };
            labelStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(labelStyle)
                { fontSize = 42, alignment = TextAnchor.MiddleCenter };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22 };
        }
        Matrix4x4 previousMatrix = GUI.matrix;
        Color previousColor = GUI.color;
        float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
        Vector3 offset = new Vector3((Screen.width - Width * scale) / 2f,
            (Screen.height - Height * scale) / 2f, 0f);
        GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(scale, scale, 1f));
        GUI.BeginGroup(new Rect(0f, 0f, Width, Height));
        DrawBox(new Rect(0f, 0f, Width, Height), new Color(0.025f, 0.04f, 0.09f));
        DrawBox(player, Color.cyan);
        // The nose is inside the player's collision rectangle.
        DrawBox(new Rect(player.center.x - 6f, player.y, 12f, 10f), Color.white);
        foreach (Rect bullet in bullets) DrawBox(bullet, Color.yellow);
        foreach (Rect enemy in enemies) DrawBox(enemy, new Color(1f, 0.25f, 0.3f));
        DrawBox(new Rect(0f, 0f, Width, 44f), new Color(0.015f, 0.02f, 0.04f));
        GUI.Label(new Rect(16f, 8f, 220f, 30f), "Score: " + score, labelStyle);
        GUI.Label(new Rect(340f, 8f, 610f, 30f),
            "WASD / Arrows: move     Space: shoot", labelStyle);
        if (gameOver)
        {
            DrawBox(new Rect(200f, 185f, 560f, 230f), new Color(0.015f, 0.02f, 0.04f));
            GUI.Label(new Rect(200f, 200f, 560f, 65f), "GAME OVER", titleStyle);
            GUI.Label(new Rect(200f, 270f, 560f, 55f), "Score: " + score, titleStyle);
            if (GUI.Button(new Rect(330f, 345f, 300f, 50f), "Restart (R)", buttonStyle))
                ResetGame();
        }
        GUI.EndGroup();
        GUI.matrix = previousMatrix;
        GUI.color = previousColor;
    }
}
