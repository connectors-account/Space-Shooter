using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Pause overlay. Shown when the game state becomes Paused. Provides Resume,
    /// Restart, Main Menu and Quit buttons plus SFX and music volume sliders.
    /// Builds its UI at runtime if not wired. Listens to the pause input via
    /// the player's input handler is not needed here – GameManager drives state
    /// while this reads it and shows/hides accordingly.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider musicSlider;

        private Player.PlayerInputHandler _input;

        private void Awake()
        {
            BuildIfNeeded();
            Hide();
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            var playerGo = GameObject.FindGameObjectWithTag(Constants.TagPlayer);
            if (playerGo != null)
                _input = playerGo.GetComponent<Player.PlayerInputHandler>();

            if (AudioManager.Instance != null)
            {
                if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SfxVolume;
                if (musicSlider != null) musicSlider.value = AudioManager.Instance.MusicVolume;
            }
        }

        private void Update()
        {
            // Toggle pause when the pause control is pressed while playing/paused.
            if (_input == null)
            {
                var playerGo = GameObject.FindGameObjectWithTag(Constants.TagPlayer);
                if (playerGo != null) _input = playerGo.GetComponent<Player.PlayerInputHandler>();
            }

            if (_input != null && _input.PausePressed && GameManager.Instance != null)
            {
                if (GameManager.Instance.IsPlaying) GameManager.Instance.PauseGame();
                else if (GameManager.Instance.IsPaused) GameManager.Instance.ResumeGame();
            }
        }

        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next == GameState.Paused) Show();
            else Hide();
        }

        private void Show()
        {
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // -----------------------------------------------------------------
        // Button actions
        // -----------------------------------------------------------------
        public void OnResumeClicked()
        {
            PlayClick();
            if (GameManager.Instance != null) GameManager.Instance.ResumeGame();
        }

        public void OnRestartClicked()
        {
            PlayClick();
            if (Scoring.ScoreManager.Instance != null) Scoring.ScoreManager.Instance.ResetForNewGame();
            if (GameManager.Instance != null) GameManager.Instance.RestartGame();
        }

        public void OnMainMenuClicked()
        {
            PlayClick();
            if (GameManager.Instance != null) GameManager.Instance.GoToMainMenu();
        }

        public void OnQuitClicked()
        {
            PlayClick();
            if (GameManager.Instance != null) GameManager.Instance.QuitGame();
            else Application.Quit();
        }

        private void OnSfxChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(value);
        }

        private void OnMusicChanged(float value)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(value);
        }

        private void PlayClick()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(Constants.SfxUiClick);
        }

        // -----------------------------------------------------------------
        // Runtime UI
        // -----------------------------------------------------------------
        private void BuildIfNeeded()
        {
            if (panelRoot != null) return;

            var canvas = UIFactory.CreateCanvas("PauseCanvas", transform, 200);
            var dim = UIFactory.CreatePanel(canvas.transform, new Color(0f, 0f, 0.05f, 0.8f));
            panelRoot = dim.gameObject;

            UIFactory.CreateText(panelRoot.transform, "PAUSED", 64, new Vector2(0f, 420f), new Vector2(600f, 100f));

            UIFactory.CreateButton(panelRoot.transform, "RESUME", new Vector2(0f, 240f), new Vector2(320f, 84f), OnResumeClicked);
            UIFactory.CreateButton(panelRoot.transform, "RESTART", new Vector2(0f, 140f), new Vector2(320f, 84f), OnRestartClicked);
            UIFactory.CreateButton(panelRoot.transform, "MAIN MENU", new Vector2(0f, 40f), new Vector2(320f, 84f), OnMainMenuClicked);
            UIFactory.CreateButton(panelRoot.transform, "QUIT", new Vector2(0f, -60f), new Vector2(320f, 84f), OnQuitClicked);

            float sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.8f;
            float music = AudioManager.Instance != null ? AudioManager.Instance.MusicVolume : 0.5f;
            sfxSlider = UIFactory.CreateLabelledSlider(panelRoot.transform, "SFX", new Vector2(0f, -200f), sfx, OnSfxChanged);
            musicSlider = UIFactory.CreateLabelledSlider(panelRoot.transform, "MUSIC", new Vector2(0f, -280f), music, OnMusicChanged);
        }
    }
}
