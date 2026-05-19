using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Main Menu scene UI and animations.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text titleText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Text controlsText;
    [SerializeField] private Text versionText;

    [Header("Animation")]
    [SerializeField] private float titleBobSpeed = 1.5f;
    [SerializeField] private float titleBobAmount = 10f;

    private Vector3 titleStartPos;

    private void Start()
    {
        // Display high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = highScore > 0 ? $"HIGH SCORE: {highScore:N0}" : "";

        if (controlsText != null)
            controlsText.text = "WASD / Arrow Keys - Move\nSpace - Shoot\nESC - Pause";

        if (versionText != null)
            versionText.text = "v1.0";

        // Button listeners
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Store title position for animation
        if (titleText != null)
            titleStartPos = titleText.rectTransform.anchoredPosition;

        // Start music
        AudioManager.Instance?.PlayMusic();
    }

    private void Update()
    {
        // Animate title
        if (titleText != null)
        {
            float y = titleStartPos.y + Mathf.Sin(Time.time * titleBobSpeed) * titleBobAmount;
            titleText.rectTransform.anchoredPosition = new Vector2(titleStartPos.x, y);
        }

        // Press Enter/Space to start
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayClicked();
        }
    }

    private void OnPlayClicked()
    {
        AudioManager.Instance?.PlaySFX("PowerUp");
        GameManager.Instance?.StartGame();
    }

    private void OnQuitClicked()
    {
        GameManager.Instance?.QuitGame();
    }
}
