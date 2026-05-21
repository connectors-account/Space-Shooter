using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause menu with Resume and Main Menu buttons.
    /// Shown when ESC is pressed during gameplay.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TextMeshProUGUI pauseTitle;

        private void Start()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            var gm = Managers.GameManager.Instance;
            if (gm != null)
                gm.OnGameStateChanged += OnGameStateChanged;

            Hide();
        }

        private void OnDestroy()
        {
            var gm = Managers.GameManager.Instance;
            if (gm != null)
                gm.OnGameStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(Managers.GameState state)
        {
            if (state == Managers.GameState.Paused)
                Show();
            else
                Hide();
        }

        private void OnResumeClicked()
        {
            Managers.GameManager.Instance?.ResumeGame();
        }

        private void OnMainMenuClicked()
        {
            Managers.GameManager.Instance?.GoToMainMenu();
        }

        public void Show()
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void Hide()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }
}
