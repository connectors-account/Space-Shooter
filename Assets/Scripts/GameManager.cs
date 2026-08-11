using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Central game state controller for the gameplay scene.
    /// Tracks score and player lives, and notifies the UIManager of changes.
    /// Persists a single instance for the active gameplay scene.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting Values")]
        [Tooltip("Number of lives the player starts with.")]
        [SerializeField] private int startingLives = 3;

        [Header("References")]
        [Tooltip("UIManager in the scene. Auto-found if left empty.")]
        [SerializeField] private UIManager uiManager;

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            // Simple scene-scoped singleton (not persistent across scenes on purpose).
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (uiManager == null)
            {
                uiManager = FindObjectOfType<UIManager>();
            }

            Score = 0;
            Lives = startingLives;
            IsGameOver = false;

            // Make sure time is running (in case we restarted from a paused game-over state).
            Time.timeScale = 1f;

            if (uiManager != null)
            {
                uiManager.UpdateScore(Score);
                uiManager.UpdateLives(Lives);
                uiManager.HideGameOver();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Add points to the score (called when an enemy is destroyed).</summary>
        public void AddScore(int amount)
        {
            if (IsGameOver)
            {
                return;
            }

            Score += amount;

            if (uiManager != null)
            {
                uiManager.UpdateScore(Score);
            }
        }

        /// <summary>Reduce player lives by one. Triggers game over at zero.</summary>
        public void PlayerHit()
        {
            if (IsGameOver)
            {
                return;
            }

            Lives = Mathf.Max(0, Lives - 1);

            if (uiManager != null)
            {
                uiManager.UpdateLives(Lives);
            }

            if (Lives <= 0)
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            IsGameOver = true;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGameOver();
            }

            if (uiManager != null)
            {
                uiManager.ShowGameOver(Score);
            }

            // Freeze gameplay. UIManager restart button resets timeScale via scene reload.
            Time.timeScale = 0f;
        }
    }
}
