using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OrbitBreaker
{
    /// <summary>Builds a complete uGUI interface in Awake; no manual Canvas or font setup required.</summary>
    public sealed class GameUI : MonoBehaviour
    {
        public GameController game;
        private Font font;
        private GameObject title, pause, gameOver, hud;
        private Text score, health, wave, powers, result, best, announcement, muteLabel;
        private float announcementUntil;
        private readonly Color cyan = new Color(.4f, .93f, 1f);
        private readonly Color muted = new Color(.64f, .72f, .83f);

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            hud = Panel("HUD", transform, new Color(0, 0, 0, 0));
            var bar = Rect("Telemetry", hud.transform, new Vector2(.5f, 1), new Vector2(0, -40), new Vector2(1220, 68));
            bar.gameObject.AddComponent<Image>().color = new Color(.025f, .055f, .11f, .91f);
            score = Label(bar, "SCORE  0", new Vector2(-400, 0), new Vector2(330, 60), 26, cyan);
            wave = Label(bar, "WAVE  1", Vector2.zero, new Vector2(220, 60), 24, Color.white);
            health = Label(bar, "HULL  5 / 5", new Vector2(400, 0), new Vector2(300, 60), 26, Color.white);
            powers = Label(hud.transform, "", new Vector2(0, -275), new Vector2(1150, 44), 19, cyan);
            Label(hud.transform, "WASD / ARROWS  MOVE     SPACE  FIRE     ESC / P  PAUSE     M  SOUND",
                new Vector2(0, -322), new Vector2(1160, 32), 16, muted);
            announcement = Label(hud.transform, "", new Vector2(0, 205), new Vector2(900, 70), 34, cyan);

            title = Panel("Title", transform, new Color(.018f, .032f, .07f, .82f));
            Label(title.transform, "ORBIT BREAKER", new Vector2(0, 205), new Vector2(1100, 100), 66, cyan);
            Label(title.transform, "A SINGLE-PILOT ARCADE SHOOTER", new Vector2(0, 133), new Vector2(1000, 48), 20, muted);
            Label(title.transform, "Survive the waves. Destroy ships for points.\nEscaped enemies damage your hull.",
                new Vector2(0, 57), new Vector2(1000, 78), 23, Color.white);
            Label(title.transform, "WASD / ARROWS to move  |  Hold SPACE to fire\nESC / P pauses  |  M toggles sound",
                new Vector2(0, -26), new Vector2(1000, 70), 20, muted);
            Label(title.transform, "+ REPAIR     LIGHTNING: RAPID FIRE     THREE ARROWS: TRIPLE SHOT",
                new Vector2(0, -96), new Vector2(1120, 45), 17, cyan);
            Button(title.transform, "LAUNCH  [ENTER]", new Vector2(0, -171), game.BeginGame);
            Button(title.transform, "QUIT", new Vector2(0, -236), game.Quit);
            best = Label(title.transform, "", new Vector2(0, -303), new Vector2(700, 40), 18, muted);

            pause = Panel("Pause", transform, new Color(.015f, .025f, .06f, .95f));
            Label(pause.transform, "FLIGHT PAUSED", new Vector2(0, 170), new Vector2(1000, 90), 48, cyan);
            Button(pause.transform, "RESUME  [ESC / P]", new Vector2(0, 70), () => game.SetPaused(false));
            Button(pause.transform, "RESTART  [R]", new Vector2(0, 5), game.BeginGame);
            Button(pause.transform, "MAIN MENU", new Vector2(0, -60), game.ReturnToTitle);
            muteLabel = Button(pause.transform, "SOUND ON  [M]", new Vector2(0, -125), game.ToggleMute);
            Button(pause.transform, "QUIT", new Vector2(0, -190), game.Quit);

            gameOver = Panel("Game Over", transform, new Color(.025f, .018f, .05f, .94f));
            Label(gameOver.transform, "SIGNAL LOST", new Vector2(0, 170), new Vector2(1000, 90), 56, new Color(1f, .48f, .43f));
            result = Label(gameOver.transform, "", new Vector2(0, 66), new Vector2(900, 110), 27, Color.white);
            Button(gameOver.transform, "TRY AGAIN  [ENTER / R]", new Vector2(0, -45), game.BeginGame);
            Button(gameOver.transform, "MAIN MENU", new Vector2(0, -110), game.ReturnToTitle);
            Button(gameOver.transform, "QUIT", new Vector2(0, -175), game.Quit);
            Refresh();
        }

        private void Update()
        {
            if (game.Phase != GamePhase.Playing && game.Phase != GamePhase.Paused) return;
            score.text = "SCORE  " + game.Score.ToString("N0");
            wave.text = "WAVE  " + game.Wave;
            if (game.Player != null)
            {
                health.text = "HULL  " + game.Player.Health + " / " + PlayerController.MaxHealth;
                health.color = game.Player.Health <= 2 ? new Color(1f, .5f, .4f) : Color.white;
                string rapid = game.Player.RapidRemaining > 0 ? "RAPID " + Mathf.CeilToInt(game.Player.RapidRemaining) + "s    " : "";
                string triple = game.Player.TripleRemaining > 0 ? "TRIPLE " + Mathf.CeilToInt(game.Player.TripleRemaining) + "s" : "";
                powers.text = rapid + triple;
            }
            if (Time.time >= announcementUntil) announcement.text = "";
        }

        public void Refresh()
        {
            if (title == null) return;
            title.SetActive(game.Phase == GamePhase.Title);
            pause.SetActive(game.Phase == GamePhase.Paused);
            gameOver.SetActive(game.Phase == GamePhase.GameOver);
            hud.SetActive(game.Phase == GamePhase.Playing || game.Phase == GamePhase.Paused);
            best.text = "PERSONAL BEST  " + game.Best.ToString("N0") + (game.Muted ? "    SOUND OFF [M]" : "    SOUND ON [M]");
            muteLabel.text = game.Muted ? "SOUND OFF  [M]" : "SOUND ON  [M]";
            result.text = "SCORE  " + game.Score.ToString("N0") + "     WAVE  " + game.Wave + "\nBEST  " + game.Best.ToString("N0");
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        public void Announce(string text) { announcement.text = text; announcementUntil = Time.time + 1.8f; }

        private static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = position;
            return rt;
        }

        private static GameObject Panel(string name, Transform parent, Color color)
        {
            var rt = Rect(name, parent, new Vector2(.5f, .5f), Vector2.zero, Vector2.zero);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = color.a > .1f;
            return rt.gameObject;
        }

        private Text Label(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var rt = Rect(text, parent, new Vector2(.5f, .5f), position, size);
            var label = rt.gameObject.AddComponent<Text>();
            label.font = font; label.text = text; label.fontSize = fontSize;
            label.color = color; label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            label.supportRichText = false;
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, .8f); shadow.effectDistance = new Vector2(1, -2);
            return label;
        }

        private Text Button(Transform parent, string text, Vector2 position, UnityAction action)
        {
            var rt = Rect(text, parent, new Vector2(.5f, .5f), position, new Vector2(360, 52));
            var image = rt.gameObject.AddComponent<Image>(); image.color = new Color(.055f, .17f, .24f, 1f);
            var button = rt.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(.65f, .95f, 1f);
            colors.pressedColor = new Color(.35f, .7f, .8f);
            button.colors = colors;
            button.onClick.AddListener(action);
            return Label(rt, text, Vector2.zero, new Vector2(354, 50), 20, Color.white);
        }
    }
}
