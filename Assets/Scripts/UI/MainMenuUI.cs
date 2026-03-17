using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Main menu controller
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Text titleText;
        
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject creditsPanel;
        
        [Header("Animation")]
        [SerializeField] private bool animateTitle = true;
        [SerializeField] private float titleBobSpeed = 2f;
        [SerializeField] private float titleBobAmount = 10f;
        
        private Vector3 titleStartPosition;
        
        private void Start()
        {
            SetupButtons();
            UpdateHighScore();
            
            if (titleText != null)
            {
                titleStartPosition = titleText.transform.position;
            }
            
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
            
            // Ensure time scale is normal
            Time.timeScale = 1f;
        }
        
        private void Update()
        {
            if (animateTitle && titleText != null)
            {
                float yOffset = Mathf.Sin(Time.time * titleBobSpeed) * titleBobAmount;
                titleText.transform.position = titleStartPosition + Vector3.up * yOffset;
            }
        }
        
        private void SetupButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            
            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(OnCreditsClicked);
            }
        }
        
        private void UpdateHighScore()
        {
            if (highScoreText != null && SpaceShooter.Core.GameManager.Instance != null)
            {
                highScoreText.text = $"High Score: {SpaceShooter.Core.GameManager.Instance.HighScore}";
            }
        }
        
        public void OnStartClicked()
        {
            if (SpaceShooter.Core.GameManager.Instance != null)
            {
                SpaceShooter.Core.GameManager.Instance.StartGame();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
            }
        }
        
        public void OnQuitClicked()
        {
            if (SpaceShooter.Core.GameManager.Instance != null)
            {
                SpaceShooter.Core.GameManager.Instance.QuitGame();
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
        
        public void OnCreditsClicked()
        {
            if (mainPanel != null)
                mainPanel.SetActive(false);
            if (creditsPanel != null)
                creditsPanel.SetActive(true);
        }
        
        public void OnBackFromCredits()
        {
            if (creditsPanel != null)
                creditsPanel.SetActive(false);
            if (mainPanel != null)
                mainPanel.SetActive(true);
        }
    }
}
