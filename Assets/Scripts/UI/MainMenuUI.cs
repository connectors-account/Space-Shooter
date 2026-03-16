using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// MainMenuUI handles the main menu screen interactions.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TextMeshProUGUI versionText;

    [Header("Animation")]
    [SerializeField] private float titleBobSpeed = 2f;
    [SerializeField] private float titleBobAmount = 10f;

    private Vector3 titleStartPosition;

    private void Start()
    {
        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        // Store title position for animation
        if (titleText != null)
        {
            titleStartPosition = titleText.transform.localPosition;
        }

        // Update high score display
        UpdateHighScoreDisplay();

        // Set version text
        if (versionText != null)
        {
            versionText.text = $"v{Application.version}";
        }

        // Start background music
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMusic("MenuMusic");
        }
    }

    private void Update()
    {
        // Animate title
        AnimateTitle();
    }

    /// <summary>
    /// Simple bob animation for title
    /// </summary>
    private void AnimateTitle()
    {
        if (titleText != null)
        {
            float yOffset = Mathf.Sin(Time.time * titleBobSpeed) * titleBobAmount;
            titleText.transform.localPosition = titleStartPosition + new Vector3(0, yOffset, 0);
        }
    }

    /// <summary>
    /// Update high score display
    /// </summary>
    private void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            highScoreText.text = $"HIGH SCORE: {highScore:N0}";
        }
    }

    /// <summary>
    /// Called when Play button is clicked
    /// </summary>
    private void OnPlayClicked()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadGameScene();
        }
    }

    /// <summary>
    /// Called when Quit button is clicked
    /// </summary>
    private void OnQuitClicked()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("ButtonClick");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
}
