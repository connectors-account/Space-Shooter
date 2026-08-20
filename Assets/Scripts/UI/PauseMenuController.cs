using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause overlay. Toggled with Escape. Provides Resume / Restart / Main Menu / Quit
    /// plus SFX and Music volume sliders wired to the AudioManager.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject panelRoot;

        [Header("Buttons")]
        public Button resumeButton;
        public Button restartButton;
        public Button mainMenuButton;
        public Button quitButton;

        [Header("Volume")]
        public Slider sfxSlider;
        public Slider musicSlider;

        private void Start()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoMainMenu);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);

            if (AudioManager.Instance != null)
            {
                if (sfxSlider != null)
                {
                    sfxSlider.value = AudioManager.Instance.sfxVolume;
                    sfxSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetSFXVolume(v));
                }
                if (musicSlider != null)
                {
                    musicSlider.value = AudioManager.Instance.musicVolume;
                    musicSlider.onValueChanged.AddListener(v => AudioManager.Instance.SetMusicVolume(v));
                }
            }

            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.Instance == null) return;
                if (GameManager.Instance.State == GameState.Playing) Pause();
                else if (GameManager.Instance.State == GameState.Paused) Resume();
            }
        }

        public void Pause()
        {
            if (GameManager.Instance != null) GameManager.Instance.PauseGame();
            if (panelRoot != null) panelRoot.SetActive(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
        }

        public void Resume()
        {
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            if (panelRoot != null) panelRoot.SetActive(false);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
        }

        private void Restart()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
            if (panelRoot != null) panelRoot.SetActive(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadGame();
        }

        private void GoMainMenu()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
            if (panelRoot != null) panelRoot.SetActive(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadMainMenu();
        }

        private void Quit()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("menu_click");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
