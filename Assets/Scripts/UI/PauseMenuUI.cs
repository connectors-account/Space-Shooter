// ============================================================================
// PauseMenuUI.cs — Pause overlay with Resume / Main Menu / Quit
// ============================================================================
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            resumeButton?.onClick.AddListener(OnResume);
            mainMenuButton?.onClick.AddListener(OnMainMenu);
            quitButton?.onClick.AddListener(OnQuit);

            if (pausePanel != null) pausePanel.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleStateChange;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleStateChange;
        }

        private void HandleStateChange(GameState state)
        {
            if (pausePanel != null)
                pausePanel.SetActive(state == GameState.Paused);
        }

        private void OnResume()  => GameManager.Instance?.ResumeGame();
        private void OnMainMenu() => GameManager.Instance?.ReturnToMainMenu();
        private void OnQuit()    => GameManager.Instance?.QuitGame();
    }
}
