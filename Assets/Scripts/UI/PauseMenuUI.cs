using SpaceShooter.Audio;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance != null)
            {
                bool shouldPause = GameManager.Instance.CurrentState != GameState.Paused;
                GameManager.Instance.SetPaused(shouldPause);
                ShowPausePanel(shouldPause);
                AudioManager.Instance?.PlayUIClick();
            }
        }

        public void OnResumeClicked()
        {
            GameManager.Instance.SetPaused(false);
            ShowPausePanel(false);
            AudioManager.Instance?.PlayUIClick();
        }

        public void OnMainMenuClicked()
        {
            GameManager.Instance.SetPaused(false);
            GameManager.Instance.ReturnToMainMenu();
            AudioManager.Instance?.PlayUIClick();
        }

        private void ShowPausePanel(bool show)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(show);
            }
        }
    }
}
