using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the score and game state, draws the UI and handles restart and quit.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; }
    public bool IsGameOver { get; private set; }
    public bool IsPlaying { get { return !IsGameOver; } }

    PlayerController player;
    Camera cam;
    GUIStyle scoreStyle;
    GUIStyle titleStyle;
    GUIStyle textStyle;
    GUIStyle buttonStyle;
    Texture2D panelTexture;
    int styledForHeight;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Initialise(PlayerController playerController)
    {
        player = playerController;
        cam = Camera.main;
        Score = 0;
        IsGameOver = false;
        player.ResetPlayer();
    }

    /// <summary>Half the visible width and height in world units.</summary>
    public Vector2 HalfScreenSize
    {
        get
        {
            if (cam == null) cam = Camera.main;
            float halfHeight = cam != null ? cam.orthographicSize : 6f;
            float aspect = cam != null ? cam.aspect : 16f / 9f;
            return new Vector2(halfHeight * aspect, halfHeight);
        }
    }

    public void AddScore(int amount)
    {
        if (IsGameOver) return;
        Score += amount;
    }

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        player.gameObject.SetActive(false);
    }

    public void Restart()
    {
        foreach (Enemy e in new List<Enemy>(Enemy.Active)) Destroy(e.gameObject);
        foreach (Bullet b in new List<Bullet>(Bullet.Active)) Destroy(b.gameObject);

        Score = 0;
        IsGameOver = false;
        player.gameObject.SetActive(true);
        player.ResetPlayer();

        EnemySpawner spawner = GetComponent<EnemySpawner>();
        if (spawner != null) spawner.ResetTimer();
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void Update()
    {
        if (IsGameOver && Input.GetKeyDown(KeyCode.R)) Restart();
    }

    void BuildStyles()
    {
        if (scoreStyle != null && styledForHeight == Screen.height) return;
        styledForHeight = Screen.height;
        float unit = Screen.height / 720f;

        scoreStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(28 * unit),
            fontStyle = FontStyle.Bold
        };
        scoreStyle.normal.textColor = Color.white;

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(56 * unit),
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        titleStyle.normal.textColor = new Color(1f, 0.35f, 0.3f);

        textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(26 * unit),
            alignment = TextAnchor.MiddleCenter
        };
        textStyle.normal.textColor = Color.white;

        buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.RoundToInt(24 * unit)
        };

        if (panelTexture == null)
        {
            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.7f));
            panelTexture.Apply();
        }
    }

    void OnGUI()
    {
        BuildStyles();
        float unit = Screen.height / 720f;

        if (!IsGameOver)
        {
            GUI.Label(new Rect(20 * unit, 15 * unit, 400 * unit, 40 * unit), "Score: " + Score, scoreStyle);
            return;
        }

        float w = 460 * unit;
        float h = 340 * unit;
        Rect panel = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);
        GUI.DrawTexture(panel, panelTexture);

        GUI.Label(new Rect(panel.x, panel.y + 25 * unit, w, 70 * unit), "GAME OVER", titleStyle);
        GUI.Label(new Rect(panel.x, panel.y + 105 * unit, w, 40 * unit), "Final score: " + Score, textStyle);

        float bw = 260 * unit;
        float bh = 50 * unit;
        float bx = panel.x + (w - bw) / 2f;
        if (GUI.Button(new Rect(bx, panel.y + 165 * unit, bw, bh), "Restart (R)", buttonStyle)) Restart();
        if (GUI.Button(new Rect(bx, panel.y + 230 * unit, bw, bh), "Quit", buttonStyle)) Quit();
    }
}
