using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter
{
    /// <summary>
    /// Handles main menu, pause menu, and game over overlays.
    /// Hook public methods to UI Buttons.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        [Header("Scene Names")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameplaySceneName = "GamePlay";

        [Header("Gameplay Overlays")]
        [SerializeField] private GameObject pauseOverlay;
        [SerializeField] private GameObject gameOverOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            HideAllOverlays();
        }

        public void OnPlayButtonPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void OnQuitButtonPressed()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        public void OnResumeButtonPressed()
        {
            GameManager.Instance?.ResumeFromPause();
        }

        public void OnRestartButtonPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameplaySceneName);
        }

        public void OnMainMenuButtonPressed()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        public void ShowPauseOverlay()
        {
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(true);
            }
        }

        public void HidePauseOverlay()
        {
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(false);
            }
        }

        public void ShowGameOverOverlay()
        {
            if (gameOverOverlay != null)
            {
                gameOverOverlay.SetActive(true);
            }
        }

        public void HideAllOverlays()
        {
            if (pauseOverlay != null)
            {
                pauseOverlay.SetActive(false);
            }

            if (gameOverOverlay != null)
            {
                gameOverOverlay.SetActive(false);
            }
        }
    }
}
