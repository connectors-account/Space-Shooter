// ============================================================
//  MainMenuController.cs  –  Main Menu scene logic
//
//  Expected Canvas hierarchy in MainMenu scene:
//   Canvas
//   └─ MainMenuPanel
//       ├─ TitleText         (TMP_Text)  "SPACE SHOOTER"
//       ├─ PlayButton        (Button)    → OnPlayClicked()
//       ├─ HighScoreText     (TMP_Text)
//       └─ QuitButton        (Button)    → OnQuitClicked()
// ============================================================
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text highScoreText;
    public TMP_Text titleText;

    // Animation state
    float _titleTimer;
    readonly Color _colA = new Color(0.4f, 0.8f, 1f);
    readonly Color _colB = new Color(0.8f, 0.4f, 1f);

    void Start()
    {
        Time.timeScale = 1f;

        // Display persisted high score
        int hs = PlayerPrefs.GetInt("SpaceShooter_HighScore", 0);
        if (highScoreText)
            highScoreText.text = hs > 0 ? $"BEST  {hs:000000}" : "BEST  ------";
    }

    void Update()
    {
        // Pulsing title colour
        if (titleText)
        {
            _titleTimer += Time.deltaTime;
            titleText.color = Color.Lerp(_colA, _colB,
                (Mathf.Sin(_titleTimer * 1.5f) + 1f) * 0.5f);
        }
    }

    // ── Button callbacks ─────────────────────────────────────

    public void OnPlayClicked()
    {
        if (GameManager.Instance == null)
        {
            // GameManager persists across scenes – create it if missing
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
        GameManager.Instance?.StartGame();
    }

    public void OnQuitClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.QuitApplication();
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
