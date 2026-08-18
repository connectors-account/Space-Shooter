using UnityEngine;
using UnityEngine.UI;

namespace SpaceShooter
{
    /// <summary>
    /// Pause overlay toggled with Escape. Freezes time while paused and provides
    /// Resume / Restart / Main Menu buttons plus music and SFX volume sliders.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        private void Start()
        {
            if (panel != null) panel.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(OnResume);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPause += ShowPanel;
                GameManager.Instance.OnResume += HidePanel;
            }

            InitVolumeSliders();
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResume);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenu);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPause -= ShowPanel;
                GameManager.Instance.OnResume -= HidePanel;
            }

            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolume);
            if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolume);
        }

        private void Update()
        {
            // Escape toggling is owned by GameManager; this is a fallback if none exists.
            if (GameManager.Instance == null && Input.GetKeyDown(KeyCode.Escape))
            {
                if (panel != null) panel.SetActive(!panel.activeSelf);
                Time.timeScale = (panel != null && panel.activeSelf) ? 0f : 1f;
            }
        }

        private void InitVolumeSliders()
        {
            if (AudioManager.Instance == null) return;

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = AudioManager.Instance.MusicSource != null
                    ? AudioManager.Instance.MusicSource.volume
                    : 0.6f;
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolume);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = AudioManager.Instance.SFXSource != null
                    ? AudioManager.Instance.SFXSource.volume
                    : 0.8f;
                sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolume);
            }
        }

        private void ShowPanel() { if (panel != null) panel.SetActive(true); }
        private void HidePanel() { if (panel != null) panel.SetActive(false); }

        private void OnResume()
        {
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
            else { Time.timeScale = 1f; HidePanel(); }
        }

        private void OnRestart()
        {
            Time.timeScale = 1f;
            HidePanel();
            if (SceneLoader.Instance != null) SceneLoader.Instance.ReloadGame();
            if (GameManager.Instance != null) GameManager.Instance.NewGame();
        }

        private void OnMainMenu()
        {
            Time.timeScale = 1f;
            HidePanel();
            if (SceneLoader.Instance != null) SceneLoader.Instance.LoadMainMenu();
            if (GameManager.Instance != null) GameManager.Instance.EnterMainMenu();
        }

        private void OnMusicVolume(float v)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(v);
        }

        private void OnSfxVolume(float v)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(v);
        }
    }
}
