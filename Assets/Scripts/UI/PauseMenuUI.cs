using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause overlay shown when the game state is Paused. Dims the background with
    /// a semi-transparent panel and offers Resume / Main Menu / Quit.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Root")]
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _dimPanel;

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private Button _quitButton;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            GameManager.OnStateChanged += HandleStateChanged;
            if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResume);
            if (_menuButton != null) _menuButton.onClick.AddListener(OnMenu);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuit);

            if (_dimPanel != null) _dimPanel.color = new Color(0f, 0f, 0f, 0.6f);
            SetVisible(false);
        }

        private void OnDisable()
        {
            GameManager.OnStateChanged -= HandleStateChanged;
            if (_resumeButton != null) _resumeButton.onClick.RemoveListener(OnResume);
            if (_menuButton != null) _menuButton.onClick.RemoveListener(OnMenu);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuit);
        }
        #endregion

        #region State
        private void HandleStateChanged(GameManager.GameState state)
        {
            SetVisible(state == GameManager.GameState.Paused);
        }

        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
            else gameObject.SetActive(visible);
        }
        #endregion

        #region Button Handlers
        private void OnResume()
        {
            Click();
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }

        private void OnMenu()
        {
            Click();
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        }

        private void OnQuit()
        {
            Click();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Click()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.ButtonClick);
        }
        #endregion
    }
}
