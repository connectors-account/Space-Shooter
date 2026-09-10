using UnityEngine;

namespace Starfall
{
    // Built-in IMGUI keeps the desktop UI dependency-free and resolution-independent.
    public sealed class GameHud : MonoBehaviour
    {
        GUIStyle title, heading, body, small, button, number;
        readonly Color cyan = new Color(0.4f, 0.9f, 1);
        readonly Color ink = new Color(0.035f, 0.06f, 0.12f, 0.96f);
        void Styles()
        {
            if (title != null) return;
            title = Text(64, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
            heading = Text(23, FontStyle.Bold, TextAnchor.MiddleCenter, cyan);
            body = Text(19, FontStyle.Normal, TextAnchor.MiddleCenter, Color.white);
            small = Text(14, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.6f, 0.7f, 0.82f));
            number = Text(24, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
            button = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
            button.normal.textColor = Color.white;
        }
        static GUIStyle Text(int size, FontStyle weight, TextAnchor align, Color color)
        {
            var s = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, alignment = align };
            s.normal.textColor = color; return s;
        }
        static void Fill(Rect rect, Color color)
        {
            Color before = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = before;
        }
        bool Button(float y, string text) { return GUI.Button(new Rect(480, y, 320, 48), text, button); }
        void Label(float y, string text, GUIStyle style) { GUI.Label(new Rect(130, y, 1020, 50), text, style); }
        void OnGUI()
        {
            var game = StarGame.Instance;
            if (game == null) return;
            Styles();
            Matrix4x4 before = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            float ox = (Screen.width - 1280 * scale) / 2, oy = (Screen.height - 720 * scale) / 2;
            Fill(new Rect(0, 0, ox, Screen.height), Color.black);
            Fill(new Rect(Screen.width - ox, 0, ox, Screen.height), Color.black);
            Fill(new Rect(0, 0, Screen.width, oy), Color.black);
            Fill(new Rect(0, Screen.height - oy, Screen.width, oy), Color.black);
            GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy, 0), Quaternion.identity, Vector3.one * scale);
            if (game.assets == null)
            {
                Label(290, "CONTENT NOT GENERATED", heading);
                Label(350, "Exit Play mode, then use Starfall > Rebuild Generated Content.", body);
                GUI.matrix = before; return;
            }
            if (game.State != RunState.Menu)
            {
                Fill(new Rect(0, 0, 1280, 65), ink);
                GUI.Label(new Rect(30, 12, 270, 40), "SCORE  " + game.Score.ToString("000000"), number);
                GUI.Label(new Rect(540, 12, 200, 40), "WAVE  " + game.Wave.ToString("00"), number);
                GUI.Label(new Rect(1020, 12, 230, 40), "HULL  " + (game.Player != null ? game.Player.Health : 0) + " / 5", number);
                if (game.Player != null)
                {
                    string powers = "";
                    if (game.Player.SpreadSeconds > 0) powers += "SPREAD " + Mathf.CeilToInt(game.Player.SpreadSeconds) + "s   ";
                    if (game.Player.RapidSeconds > 0) powers += "RAPID " + Mathf.CeilToInt(game.Player.RapidSeconds) + "s";
                    GUI.Label(new Rect(30, 66, 700, 34), powers, number);
                }
                if (!string.IsNullOrEmpty(game.Banner) && game.State == RunState.Playing) Label(160, game.Banner, heading);
            }
            if (game.State == RunState.Menu)
            {
                Fill(new Rect(270, 85, 740, 570), ink);
                Fill(new Rect(270, 85, 740, 3), cyan);
                Label(120, "S T A R F A L L", title);
                Label(195, "A SMALL SHIP. AN ENDLESS SKY.", heading);
                Label(245, "Dodge three enemy patterns. Collect upgrades. Survive the waves.", body);
                Label(290, "BEST  " + game.Best.ToString("000000"), heading);
                if (Button(365, "LAUNCH  /  ENTER")) game.StartRun();
                if (Button(425, game.Muted ? "SOUND: OFF  /  M" : "SOUND: ON  /  M")) game.ToggleMute();
                if (Button(485, "QUIT")) game.Quit();
                Label(555, "WASD / ARROWS  move     SPACE / Z  fire     SHIFT  precision     ESC  pause", small);
                Label(590, "Green +: repair     Gold bolt: rapid fire     Violet trident: spread shot", small);
            }
            else if (game.State == RunState.Paused || game.State == RunState.GameOver)
            {
                Fill(new Rect(0, 65, 1280, 655), new Color(0.01f, 0.02f, 0.05f, 0.78f));
                bool paused = game.State == RunState.Paused;
                Label(155, paused ? "PAUSED" : "SIGNAL LOST", title);
                Label(235, paused ? "Take a breath. The stars can wait." : "SCORE " + game.Score + "     BEST " + game.Best + "     WAVE " + game.Wave, body);
                if (Button(325, paused ? "RESUME  /  ESC" : "TRY AGAIN  /  ENTER")) { if (paused) game.Resume(); else game.StartRun(); }
                if (Button(385, "MAIN MENU")) game.Menu();
                if (Button(445, game.Muted ? "SOUND: OFF  /  M" : "SOUND: ON  /  M")) game.ToggleMute();
                if (Button(505, "QUIT")) game.Quit();
            }
            GUI.matrix = before;
        }
    }
}
