using UnityEngine;

namespace Starfall
{
    // Small desktop UI implemented with built-in IMGUI: no font or UI package dependency.
    public sealed class GameUI : MonoBehaviour
    {
        public Texture2D playerArt;
        private GUIStyle title, heading, body, small, number, button;
        private readonly Color cyan = new Color(0.35f, 0.9f, 1f);
        private readonly Color muted = new Color(0.55f, 0.66f, 0.78f);
        private const float Width = 900f;
        private const float Height = 1125f;

        private void CreateStyles()
        {
            if (title != null) return;
            title = Label(64, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            heading = Label(24, cyan, TextAnchor.MiddleCenter, FontStyle.Bold);
            body = Label(20, Color.white, TextAnchor.MiddleCenter);
            small = Label(16, muted, TextAnchor.MiddleCenter);
            number = Label(26, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            button = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            button.normal.background = Texture2D.whiteTexture;
            button.hover.background = Texture2D.whiteTexture;
            button.active.background = Texture2D.whiteTexture;
            button.focused.background = Texture2D.whiteTexture;
            button.normal.textColor = button.hover.textColor = button.active.textColor = button.focused.textColor = new Color(0.02f, 0.06f, 0.1f);
        }

        private static GUIStyle Label(int size, Color color, TextAnchor alignment, FontStyle font = FontStyle.Normal)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, alignment = alignment, fontStyle = font, wordWrap = true };
            style.normal.textColor = color;
            return style;
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private bool Button(float y, string text, bool primary = true)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = primary ? cyan : new Color(0.55f, 0.67f, 0.78f);
            bool pressed = GUI.Button(new Rect(265, y, 370, 54), text, button);
            GUI.backgroundColor = previous;
            return pressed;
        }

        private void OnGUI()
        {
            if (GameController.Instance == null) return;
            CreateStyles();
            GameController game = GameController.Instance;
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / Width, Screen.height / Height);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - Width * scale) / 2f, (Screen.height - Height * scale) / 2f, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            if (game.State != GameState.Title) Hud(game);
            if (game.State == GameState.Playing)
            {
                if (game.Announcement.Length > 0) GUI.Label(new Rect(120, 150, 660, 54), game.Announcement, heading);
                if (GUI.Button(new Rect(765, 30, 100, 36), "PAUSE", GUI.skin.button)) game.Pause();
            }
            else
            {
                Fill(new Rect(0, 0, Width, Height), new Color(0.01f, 0.025f, 0.06f, 0.8f));
                Fill(new Rect(180, 185, 540, 750), new Color(0.035f, 0.065f, 0.11f, 0.98f));
                Fill(new Rect(180, 185, 540, 3), cyan);
                if (game.State == GameState.Title) Title(game);
                else if (game.State == GameState.Paused) Pause(game);
                else GameOver(game);
            }
            GUI.matrix = previous;
        }

        private void Hud(GameController game)
        {
            Fill(new Rect(0, 0, Width, 115), new Color(0.02f, 0.04f, 0.075f, 0.94f));
            GUI.Label(new Rect(30, 13, 250, 26), "SCORE", small);
            GUI.Label(new Rect(60, 39, 220, 42), game.Score.ToString("D6"), number);
            GUI.Label(new Rect(340, 24, 220, 40), "WAVE " + game.Wave.ToString("D2"), heading);
            PlayerShip player = game.Player;
            if (player == null) return;
            for (int i = 0; i < PlayerShip.MaxHealth; i++)
                Fill(new Rect(355 + i * 40, 78, 30, 8), i < player.Health ? cyan : new Color(0.14f, 0.2f, 0.28f));
            Fill(new Rect(0, 1065, Width, 60), new Color(0.02f, 0.04f, 0.075f, 0.9f));
            string rapid = player.RapidSeconds > 0 ? "TRIPLE " + Mathf.CeilToInt(player.RapidSeconds) + "s" : "STANDARD SHOT";
            string shield = player.ShieldHits > 0 ? "SHIELD " + player.ShieldHits + " / " + Mathf.CeilToInt(player.ShieldSeconds) + "s" : "SHIELD OFF";
            GUI.Label(new Rect(20, 1074, 860, 35), rapid + "     |     " + shield + "     |     " + (player.MouseControl ? "MOUSE" : "KEYBOARD") + " [M]", small);
        }

        private void Title(GameController game)
        {
            GUI.Label(new Rect(200, 216, 500, 35), "S E C T O R   D E F E N S E", small);
            GUI.Label(new Rect(200, 262, 500, 90), "STARFALL", title);
            if (playerArt != null) GUI.DrawTexture(new Rect(380, 355, 140, 140), playerArt, ScaleMode.ScaleToFit, true);
            GUI.Label(new Rect(225, 501, 450, 38), "Hold the line. Survive the next wave.", body);
            if (Button(560, "LAUNCH  /  ENTER")) game.StartGame();
            if (Button(628, game.audioSystem.Muted ? "SOUND: OFF" : "SOUND: ON", false)) game.audioSystem.ToggleMute();
            if (Button(696, "QUIT TO DESKTOP", false)) game.Quit();
            GUI.Label(new Rect(205, 775, 490, 112), "WASD / arrows: move   |   Space / left click: fire\nM: switch keyboard / mouse steering\nEsc / P: pause   |   Collect glowing pickups\nEscaping enemies cost one hull point.", small);
            GUI.Label(new Rect(225, 895, 450, 28), "PERSONAL BEST  " + game.Best.ToString("D6"), small);
        }

        private void Pause(GameController game)
        {
            GUI.Label(new Rect(200, 276, 500, 90), "PAUSED", title);
            GUI.Label(new Rect(230, 382, 440, 68), "Take a breath. Your sector can wait.", body);
            if (Button(498, "RESUME  /  ESC")) game.Resume();
            if (Button(566, "RESTART", false)) game.StartGame();
            if (Button(634, game.audioSystem.Muted ? "SOUND: OFF" : "SOUND: ON", false)) game.audioSystem.ToggleMute();
            if (Button(702, "TITLE SCREEN", false)) game.ShowTitle();
            if (Button(770, "QUIT TO DESKTOP", false)) game.Quit();
        }

        private void GameOver(GameController game)
        {
            GUI.Label(new Rect(200, 252, 500, 90), "SIGNAL LOST", title);
            GUI.Label(new Rect(240, 370, 420, 45), "FINAL SCORE", heading);
            GUI.Label(new Rect(200, 415, 500, 90), game.Score.ToString("D6"), title);
            GUI.Label(new Rect(230, 520, 440, 45), "WAVE " + game.Wave + "   /   BEST " + game.Best.ToString("D6"), body);
            if (Button(620, "TRY AGAIN  /  ENTER")) game.StartGame();
            if (Button(688, "TITLE SCREEN", false)) game.ShowTitle();
            if (Button(756, "QUIT TO DESKTOP", false)) game.Quit();
            GUI.Label(new Rect(230, 844, 440, 42), "Repair +2   /   Triple shot   /   Shield 3 hits", small);
        }
    }
}
