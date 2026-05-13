// ============================================================================
// MainMenuUI.cs — Main menu screen (Start / Quit)
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;

        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        private void Start()
        {
            startButton?.onClick.AddListener(OnStartClicked);
            quitButton?.onClick.AddListener(OnQuitClicked);

            if (titleText != null) titleText.text = "STAR BLASTER";
            if (highScoreText != null && GameManager.Instance != null)
                highScoreText.text = $"HIGH SCORE: {GameManager.Instance.HighScore:N0}";

            // Ensure time is running on menu
            Time.timeScale = 1f;
        }

        private void OnStartClicked()
        {
            GameManager.Instance?.StartGame();
        }

        private void OnQuitClicked()
        {
            GameManager.Instance?.QuitGame();
        }
    }
}
