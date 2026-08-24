using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause menu. ESC toggles pause via the GameManager. Provides Resume, Restart and
    /// Main Menu buttons and dims the background with a semi-transparent panel.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup dimGroup;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResume);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenu);
        }

        private void Update()
        {
            if (WasPausePressed())
            {
                TogglePause();
            }
        }

        private bool WasPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        private void TogglePause()
        {
            if (!GameManager.HasInstance)
            {
                return;
            }

            var gm = GameManager.Instance;
            if (gm.State == GameState.Playing)
            {
                gm.PauseGame();
                Show();
            }
            else if (gm.State == GameState.Paused)
            {
                gm.ResumeGame();
                Hide();
            }
        }

        public void Show()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
            if (dimGroup != null)
            {
                dimGroup.alpha = 1f;
            }
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
            if (dimGroup != null)
            {
                dimGroup.alpha = 0f;
            }
        }

        private void OnResume()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.ResumeGame();
            }
            Hide();
        }

        private void OnRestart()
        {
            if (GameManager.HasInstance)
            {
                GameManager.Instance.RestartGame();
            }
            Hide();
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadGameScene();
            }
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;
            if (GameManager.HasInstance)
            {
                GameManager.Instance.SetMenuState();
            }
            Hide();
            if (SceneLoader.HasInstance)
            {
                SceneLoader.Instance.LoadMainMenu();
            }
        }
    }
}
