using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StellarShooter
{
    public sealed class GameUI : MonoBehaviour
    {
        public GameSession game;
        public GameObject mainPanel, pausePanel, overPanel, hud;
        public Text stats, powers, announcement, result, best, soundLabel;
        public Button play, mainQuit, resume, pauseRestart, pauseMenu, pauseQuit;
        public Button retry, overMenu, overQuit, mute;
        private float messageUntil;

        private void Awake()
        {
            Bind(play, game.StartGame);
            Bind(mainQuit, game.Quit);
            Bind(resume, game.Resume);
            Bind(pauseRestart, game.StartGame);
            Bind(pauseMenu, game.MainMenu);
            Bind(pauseQuit, game.Quit);
            Bind(retry, game.StartGame);
            Bind(overMenu, game.MainMenu);
            Bind(overQuit, game.Quit);
            Bind(mute, game.sound.ToggleMute);
        }
        private void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.AddListener(() => game.sound.Play(SoundCue.Click));
            button.onClick.AddListener(action);
        }
        public void Show(GamePhase phase)
        {
            mainPanel.SetActive(phase == GamePhase.Menu);
            pausePanel.SetActive(phase == GamePhase.Paused);
            overPanel.SetActive(phase == GamePhase.GameOver);
            hud.SetActive(phase != GamePhase.Menu);
            announcement.text = "";
            best.text = "PERSONAL BEST   " + game.Best.ToString("N0");
            result.text = "SCORE  " + game.Score.ToString("N0") + "     /     WAVE  " + game.Wave + "\nBEST  " + game.Best.ToString("N0");
            Button focus = phase == GamePhase.Menu ? play : phase == GamePhase.Paused ? resume : phase == GamePhase.GameOver ? retry : null;
            EventSystem.current.SetSelectedGameObject(focus == null ? null : focus.gameObject);
        }
        public void Announce(string message, float seconds)
        {
            announcement.text = message;
            messageUntil = Time.time + seconds;
        }
        private void Update()
        {
            soundLabel.text = game.sound.Muted ? "M  /  SOUND OFF" : "M  /  SOUND ON";
            if (game.Phase == GamePhase.Menu || game.Player == null) return;
            stats.text = "SCORE  " + game.Score.ToString("N0") + "     WAVE  " + game.Wave + "     HULL  " + game.Player.Health + " / " + GameRules.MaxHealth;
            powers.text = "SPREAD  " + Status(game.Player.SpreadRemaining) + "     SHIELD  " + Status(game.Player.ShieldRemaining) + "     ESC / P  PAUSE";
            if (Time.time > messageUntil) announcement.text = "";
        }
        private static string Status(float remaining) => remaining > 0 ? Mathf.CeilToInt(remaining) + "s" : "--";
    }
}
