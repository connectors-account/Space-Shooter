using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Game over screen controller
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text newHighScoreText;
        [SerializeField] private Text statsText;
        
        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Animation")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float scoreCountDuration = 1f;
        
        private CanvasGroup canvasGroup;
        private bool isShowing = false;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        private void Start()
        {
            SetupButtons();
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
            
            if (newHighScoreText != null)
            {
                newHighScoreText.gameObject.SetActive(false);
            }
            
            // Subscribe to game state changes
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
        }
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        
        private void SetupButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
            {
                ShowGameOver(false);
            }
            else if (state == GameState.Victory)
            {
                ShowGameOver(true);
            }
            else if (state == GameState.Playing)
            {
                HideGameOver();
            }
        }
        
        public void ShowGameOver(bool isVictory)
        {
            if (isShowing) return;
            isShowing = true;
            
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            
            // Set title
            if (titleText != null)
            {
                titleText.text = isVictory ? "VICTORY!" : "GAME OVER";
                titleText.color = isVictory ? Color.green : Color.red;
            }
            
            // Display scores
            if (GameManager.Instance != null)
            {
                int finalScore = GameManager.Instance.CurrentScore;
                int highScore = GameManager.Instance.HighScore;
                
                if (scoreText != null)
                {
                    scoreText.text = $"Final Score: {finalScore:N0}";
                }
                
                if (highScoreText != null)
                {
                    highScoreText.text = $"High Score: {highScore:N0}";
                }
                
                // Check for new high score
                if (newHighScoreText != null)
                {
                    bool isNewHighScore = finalScore >= highScore && finalScore > 0;
                    newHighScoreText.gameObject.SetActive(isNewHighScore);
                }
                
                // Display stats
                if (statsText != null)
                {
                    statsText.text = $"Enemies Destroyed: {GameManager.Instance.EnemiesKilled}\n" +
                                    $"Wave Reached: {GameManager.Instance.CurrentWave}";
                }
            }
            
            // Animate fade in
            StartCoroutine(FadeIn());
        }
        
        private void HideGameOver()
        {
            isShowing = false;
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        
        private System.Collections.IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            
            canvasGroup.alpha = 0f;
            float elapsed = 0f;
            
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        public void OnRestartClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
        
        public void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReturnToMainMenu();
            }
        }
        
        public void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
