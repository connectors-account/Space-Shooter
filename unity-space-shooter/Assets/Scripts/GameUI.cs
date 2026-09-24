using UnityEngine;

namespace Starward
{
    // IMGUI keeps this small desktop project dependency-free. No UI package or EventSystem needed.
    public sealed class GameUI : MonoBehaviour
    {
        public GameManager game;
        GUIStyle title, subtitle, text, small, button, hud;
        void BuildStyles()
        {
            title = Label(58, new Color(.6f, .95f, 1));
            subtitle = Label(22, new Color(.6f, .75f, .9f));
            text = Label(24, Color.white);
            small = Label(17, new Color(.7f, .8f, .9f));
            hud = Label(21, Color.white);
            button = new GUIStyle(GUI.skin.button);
            button.fontSize = 22;
            button.fontStyle = FontStyle.Bold;
            button.normal.textColor = Color.white;
            button.padding = new RectOffset(12, 12, 8, 8);
        }
        static GUIStyle Label(int size, Color color)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = size;
            style.normal.textColor = color;
            style.alignment = TextAnchor.MiddleCenter;
            return style;
        }
        void OnGUI()
        {
            if (game == null) return;
            if (title == null) BuildStyles();
            Matrix4x4 previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            if (game.Phase != GamePhase.Menu) DrawHud();
            if (game.Phase == GamePhase.Menu) DrawMenu();
            if (game.Phase == GamePhase.Paused) DrawPaused();
            if (game.Phase == GamePhase.GameOver) DrawGameOver();
            if (game.IsPlaying)
            {
                GUI.Label(new Rect(100, 680, 1080, 30), "WASD / ARROWS  Move     SPACE  Fire     ESC / P  Pause     M  Mute", small);
                if (game.director.Intermission) GUI.Label(new Rect(290, 280, 700, 80), "NEXT WAVE INCOMING", subtitle);
            }
            GUI.matrix = previous;
        }
        void DrawHud()
        {
            Panel(new Rect(0, 0, 1280, 60), .85f);
            GUI.Label(new Rect(20, 12, 200, 35), "SCORE  " + game.Score.ToString("D6"), hud);
            GUI.Label(new Rect(220, 12, 200, 35), "BEST  " + game.Best.ToString("D6"), hud);
            GUI.Label(new Rect(435, 12, 220, 35), "WAVE  " + game.Wave + "  |  " + game.director.Remaining + " LEFT", hud);
            if (game.Player != null)
            {
                GUI.Label(new Rect(665, 12, 190, 35), "HULL  " + game.Player.Health.Current + " / 5", hud);
                GUI.Label(new Rect(870, 12, 170, 35), "WEAPON  " + game.Player.WeaponLevel + " / 3", hud);
            }
            if (GUI.Button(new Rect(1090, 12, 165, 35), game.audioBank.Muted ? "SOUND OFF" : "SOUND ON", button)) game.audioBank.ToggleMute();
        }
        void DrawMenu()
        {
            Panel(new Rect(255, 65, 770, 595), .91f);
            GUI.Label(new Rect(270, 90, 740, 85), "STARWARD", title);
            GUI.Label(new Rect(270, 175, 740, 40), "HOLD THE LINE", subtitle);
            GUI.Label(new Rect(300, 235, 680, 65), "Destroy the fleet. Survive escalating waves.\nEscaped enemies damage your hull.", text);
            GUI.Label(new Rect(300, 322, 680, 55), "WASD / Arrows: move     Space: fire\nEsc / P: pause     M: mute", small);
            if (GUI.Button(new Rect(440, 403, 400, 62), "START RUN  [ENTER]", button)) game.StartRun();
            if (GUI.Button(new Rect(440, 480, 400, 48), "QUIT TO DESKTOP", button)) game.Quit();
            GUI.Label(new Rect(300, 550, 680, 60), "Green capsule: repair +2    Gold capsule: weapon upgrade\nBEST  " + game.Best.ToString("D6"), small);
        }
        void DrawPaused()
        {
            Panel(new Rect(355, 165, 570, 400), .95f);
            GUI.Label(new Rect(375, 190, 530, 75), "PAUSED", title);
            if (GUI.Button(new Rect(440, 300, 400, 58), "RESUME  [ESC / P]", button)) game.Resume();
            if (GUI.Button(new Rect(440, 380, 400, 58), "MAIN MENU", button)) game.MainMenu();
            GUI.Label(new Rect(375, 475, 530, 45), "The run pauses when the window loses focus.", small);
        }
        void DrawGameOver()
        {
            Panel(new Rect(320, 150, 640, 435), .95f);
            GUI.Label(new Rect(340, 172, 600, 80), "SIGNAL LOST", title);
            GUI.Label(new Rect(340, 270, 600, 55), "Score " + game.Score + "    |    Wave " + game.Wave + "    |    Best " + game.Best, text);
            if (GUI.Button(new Rect(440, 357, 400, 60), "RESTART  [ENTER / R]", button)) game.StartRun();
            if (GUI.Button(new Rect(440, 440, 400, 52), "MAIN MENU", button)) game.MainMenu();
        }
        static void Panel(Rect rect, float alpha)
        {
            Color before = GUI.color;
            GUI.color = new Color(.025f, .045f, .10f, alpha);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = before;
        }
    }
}
