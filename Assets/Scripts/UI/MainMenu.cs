using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI highScoreText;
    public Button playButton;
    public Button quitButton;
    public TextMeshProUGUI versionText;

    [Header("Animation")]
    public float titlePulseSpeed = 1f;
    public float titlePulseAmount = 0.05f;

    private Vector3 titleOriginalScale;

    private void Start()
    {
        if (titleText != null)
        {
            titleOriginalScale = titleText.transform.localScale;
        }

        UpdateHighScore();
        SetupButtons();

        if (versionText != null)
        {
            versionText.text = $"v{Application.version}";
        }

        // Make sure time is running
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Animate title
        if (titleText != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * titlePulseSpeed) * titlePulseAmount;
            titleText.transform.localScale = titleOriginalScale * pulse;
        }
    }

    private void UpdateHighScore()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"High Score: {highScore:N0}";
        }
    }

    private void SetupButtons()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnPlayClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
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
