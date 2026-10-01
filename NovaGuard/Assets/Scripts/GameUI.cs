using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NovaGuard
{
    /// <summary>Resolution-scaled desktop UI. All menu callbacks are bound in source, not fragile scene events.</summary>
    public sealed class GameUI : MonoBehaviour
    {
        private GameController game;
        private Font font;
        private RectTransform canvasRoot;
        private GameObject menu, hud, paused, over;
        private Button startButton, resumeButton, restartButton;
        private Text score, hull, wave, buffs, best, results, announcement, muteLabel;
        private float announcementUntil;
        private readonly Color accent = new Color(.25f, .9f, 1);
        private readonly Color secondary = new Color(.59f, .69f, .81f);

        public void Bind(GameController controller)
        {
            game = controller;
            Build();
        }

        private void Build()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Interface", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 1;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = .5f;
            canvasRoot = (RectTransform)canvasObject.transform;

            menu = Panel("Main menu", .80f);
            Label(menu.transform, "N O V A   G U A R D", new Vector2(0, 200), new Vector2(900, 90), 58, accent);
            Label(menu.transform, "AN ENDLESS ARCADE SPACE SHOOTER", new Vector2(0, 133), new Vector2(850, 35), 19, secondary);
            Label(menu.transform, "Move. Dodge. Hold the line.", new Vector2(0, 82), new Vector2(750, 40), 25, Color.white);
            best = Label(menu.transform, "", new Vector2(0, 35), new Vector2(700, 35), 19, accent);
            startButton = MakeButton(menu.transform, "START FLIGHT", new Vector2(0, -30), new Vector2(340, 58), game.StartRun);
            MakeButton(menu.transform, "QUIT TO DESKTOP", new Vector2(0, -105), new Vector2(340, 50), game.Quit);
            Label(menu.transform, "WASD / ARROWS  Move     SPACE / Z  Fire\nESC / P  Pause     M  Mute", new Vector2(0, -196), new Vector2(960, 70), 19, secondary);
            Label(menu.transform, "Escaped enemies damage your hull. Collect repair, triple-shot and shield pickups.\nMenus: mouse, or arrows / Tab and Enter.", new Vector2(0, -284), new Vector2(1100, 60), 17, secondary);

            hud = Panel("Flight HUD", 0);
            Image topBar = Box(hud.transform, "HUD backing", new Vector2(0, 320), new Vector2(1280, 80), new Color(.018f, .038f, .085f, .92f));
            topBar.raycastTarget = false;
            hull = Label(hud.transform, "", new Vector2(-490, 320), new Vector2(250, 50), 26, accent);
            score = Label(hud.transform, "", new Vector2(-165, 320), new Vector2(360, 50), 26, Color.white);
            wave = Label(hud.transform, "", new Vector2(180, 320), new Vector2(260, 50), 23, secondary);
            MakeButton(hud.transform, "PAUSE", new Vector2(405, 320), new Vector2(135, 42), game.Pause);
            Button mute = MakeButton(hud.transform, "SOUND", new Vector2(560, 320), new Vector2(135, 42), game.sound.ToggleMute);
            muteLabel = mute.GetComponentInChildren<Text>();
            announcement = Label(hud.transform, "", new Vector2(0, 231), new Vector2(1000, 65), 32, accent);
            buffs = Label(hud.transform, "", new Vector2(0, -329), new Vector2(1220, 46), 18, Color.white);
            Box(hud.transform, "Footer backing", new Vector2(0, -329), new Vector2(1280, 62), new Color(.018f, .038f, .085f, .9f)).transform.SetAsFirstSibling();

            paused = Panel("Pause menu", .88f);
            Label(paused.transform, "FLIGHT PAUSED", new Vector2(0, 180), new Vector2(800, 80), 47, accent);
            Label(paused.transform, "Take a breath. The stars can wait.", new Vector2(0, 111), new Vector2(800, 40), 22, secondary);
            resumeButton = MakeButton(paused.transform, "RESUME", new Vector2(0, 35), new Vector2(340, 56), game.Resume);
            MakeButton(paused.transform, "RESTART FLIGHT", new Vector2(0, -37), new Vector2(340, 52), game.StartRun);
            MakeButton(paused.transform, "MAIN MENU", new Vector2(0, -107), new Vector2(340, 52), game.MainMenu);
            MakeButton(paused.transform, "QUIT TO DESKTOP", new Vector2(0, -177), new Vector2(340, 52), game.Quit);
            Label(paused.transform, "ESC / P  Resume     M  Toggle sound", new Vector2(0, -262), new Vector2(900, 40), 19, secondary);

            over = Panel("Game over", .90f);
            Label(over.transform, "SIGNAL LOST", new Vector2(0, 190), new Vector2(900, 80), 54, new Color(1, .4f, .35f));
            results = Label(over.transform, "", new Vector2(0, 83), new Vector2(1000, 100), 27, Color.white);
            restartButton = MakeButton(over.transform, "FLY AGAIN", new Vector2(0, -32), new Vector2(340, 58), game.StartRun);
            MakeButton(over.transform, "MAIN MENU", new Vector2(0, -106), new Vector2(340, 52), game.MainMenu);
            MakeButton(over.transform, "QUIT TO DESKTOP", new Vector2(0, -176), new Vector2(340, 52), game.Quit);
            Label(over.transform, "R  Restart     M  Toggle sound", new Vector2(0, -264), new Vector2(900, 40), 19, secondary);
        }

        public void Show(GamePhase phase)
        {
            if (menu == null) return;
            menu.SetActive(phase == GamePhase.Menu);
            hud.SetActive(phase != GamePhase.Menu);
            paused.SetActive(phase == GamePhase.Paused);
            over.SetActive(phase == GamePhase.GameOver);
            best.text = "PERSONAL BEST  " + game.BestScore.ToString("N0");
            results.text = "SCORE  " + game.Score.ToString("N0") + "     /     WAVE  " + game.Wave
                + "\nPERSONAL BEST  " + game.BestScore.ToString("N0");
            if (phase == GamePhase.Playing || phase == GamePhase.Menu)
            {
                announcement.text = "";
                announcementUntil = 0;
            }
            // The underlying HUD cannot consume navigation during a modal menu.
            foreach (Button button in hud.GetComponentsInChildren<Button>(true))
                button.interactable = phase == GamePhase.Playing;
            GameObject selected = phase == GamePhase.Menu ? startButton.gameObject
                : phase == GamePhase.Paused ? resumeButton.gameObject
                : phase == GamePhase.GameOver ? restartButton.gameObject : null;
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(selected);
                EventSystem.current.sendNavigationEvents = phase != GamePhase.Playing;
            }
        }

        public void Announce(string text, float duration)
        {
            announcement.text = text;
            announcementUntil = Time.time + duration;
        }

        private void Update()
        {
            if (game == null) return;
            score.text = "SCORE  " + game.Score.ToString("N0");
            wave.text = "WAVE  " + game.Wave;
            muteLabel.text = game.sound.Muted ? "MUTED" : "SOUND ON";
            if (game.Player != null)
            {
                hull.text = "HULL  " + game.Player.Health + " / " + game.Player.maxHealth;
                string rapid = game.Player.RapidRemaining > 0 ? "   TRIPLE " + Mathf.CeilToInt(game.Player.RapidRemaining) + "s" : "";
                string shield = game.Player.ShieldRemaining > 0 ? "   SHIELD " + Mathf.CeilToInt(game.Player.ShieldRemaining) + "s" : "";
                buffs.text = "DODGE FIRE / STOP ESCAPING SHIPS   |   SPACE TO FIRE" + rapid + shield;
            }
            if (Time.time >= announcementUntil) announcement.text = "";
            if (game.Phase != GamePhase.Playing && Input.GetKeyDown(KeyCode.Tab)) SelectNext();
        }

        private void SelectNext()
        {
            if (EventSystem.current == null) return;
            GameObject root = game.Phase == GamePhase.Menu ? menu : game.Phase == GamePhase.Paused ? paused : over;
            Button[] buttons = root.GetComponentsInChildren<Button>();
            GameObject current = EventSystem.current.currentSelectedGameObject;
            int index = -1;
            for (int i = 0; i < buttons.Length; i++) if (buttons[i].gameObject == current) index = i;
            bool backwards = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            index = (index + (backwards ? buttons.Length - 1 : 1)) % buttons.Length;
            EventSystem.current.SetSelectedGameObject(buttons[index].gameObject);
        }

        private GameObject Panel(string name, float opacity)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasRoot, false);
            RectTransform rect = (RectTransform)panel.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Image image = panel.GetComponent<Image>();
            image.color = new Color(.015f, .025f, .06f, opacity);
            image.raycastTarget = opacity > 0;
            return panel;
        }

        private RectTransform Position(GameObject item, Transform parent, Vector2 position, Vector2 size)
        {
            item.transform.SetParent(parent, false);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        private Image Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            var item = new GameObject(name, typeof(RectTransform), typeof(Image));
            Position(item, parent, position, size);
            Image image = item.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private Text Label(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var item = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(Shadow));
            Position(item, parent, position, size);
            Text text = item.GetComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            item.GetComponent<Shadow>().effectColor = new Color(0, 0, 0, .8f);
            return text;
        }

        private Button MakeButton(Transform parent, string title, Vector2 position, Vector2 size, UnityAction action)
        {
            Image image = Box(parent, title, position, size, new Color(.075f, .17f, .25f, 1));
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(.5f, .92f, 1);
            colors.selectedColor = new Color(.5f, .92f, 1);
            colors.pressedColor = new Color(.25f, .7f, .85f);
            button.colors = colors;
            Label(image.transform, title, Vector2.zero, size, size.y > 45 ? 21 : 17, Color.white);
            button.onClick.AddListener(() => { game.sound.Play(SoundCue.Click); action(); });
            return button;
        }
    }
}
