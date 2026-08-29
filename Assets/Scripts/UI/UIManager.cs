using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Central hub for in-game UI. Delegates HUD updates to the HUDController and manages
    /// the pause and game-over panels.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [Header("Controllers")]
        [SerializeField] private HUDController hud;
        [SerializeField] private PauseMenuController pauseMenu;
        [SerializeField] private GameOverController gameOverPanel;

        protected override bool PersistAcrossScenes => false;

        // ------------------------------------------------------------------
        // HUD
        // ------------------------------------------------------------------
        public void UpdateScore(int score) => hud?.SetScore(score);
        public void UpdateLives(int lives) => hud?.SetLives(lives);
        public void UpdateHealth(int current, int max) => hud?.SetHealth(current, max);
        public void UpdateWave(int wave) => hud?.SetWave(wave);

        public void ShowBossHealthBar(float percent) => hud?.ShowBossHealthBar(percent);
        public void HideBossHealthBar() => hud?.HideBossHealthBar();

        public void ShowMessage(string message, float duration) => hud?.ShowMessage(message, duration);

        public void ShowScorePopup(int amount, Vector3 worldPosition) => hud?.ShowScorePopup(amount, worldPosition);

        // ------------------------------------------------------------------
        // Panels
        // ------------------------------------------------------------------
        public void ShowGameOver(int score, int highScore)
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.Show(score, highScore);
            }
        }

        public void ShowPauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.Show();
            }
        }

        public void HidePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.Hide();
            }
        }
    }
}
