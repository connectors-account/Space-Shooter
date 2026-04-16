using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;

        private void Start()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(state == GameState.Paused);
            }
        }

        public void OnResumeClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        public void OnMainMenuClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.BackToMainMenu();
            }
        }
    }
}
