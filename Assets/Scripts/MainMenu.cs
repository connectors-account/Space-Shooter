using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceShooter
{
    /// <summary>
    /// Controls the Main Menu scene: Start button loads the gameplay scene,
    /// Quit button exits the standalone build.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("Name of the gameplay scene to load on Start.")]
        [SerializeField] private string gameSceneName = "Game";

        private void Start()
        {
            // Ensure normal time flow when returning to the menu from a paused game-over.
            Time.timeScale = 1f;
        }

        /// <summary>Hooked to the Start button OnClick in the Inspector.</summary>
        public void StartGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        /// <summary>Hooked to the Quit button OnClick in the Inspector.</summary>
        public void QuitGame()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
