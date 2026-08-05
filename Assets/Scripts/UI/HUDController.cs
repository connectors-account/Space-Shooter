using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;
using SpaceShooter.Scoring;
using SpaceShooter.Player;
using SpaceShooter.Enemy;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;
using SpaceShooter.PowerUps;

namespace SpaceShooter.UI
{
    /// <summary>
    /// In-game heads-up display. Subscribes to ScoreManager, PlayerHealth,
    /// WaveManager, Bomb and boss events and updates: score, multiplier, lives
    /// icons, health/lives bar, wave label, the (normally hidden) boss health
    /// bar and a row of five power-up timer slots.
    ///
    /// If UI references are not wired in the Inspector the HUD builds a
    /// functional overlay at runtime, so the game is playable with no manual
    /// canvas setup.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Optional references (auto-built if left empty)")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text multiplierText;
        [SerializeField] private Text waveText;
        [SerializeField] private Text bombText;
        [SerializeField] private Slider healthBar;
        [SerializeField] private Transform livesIconParent; // kept for Inspector wiring
        [SerializeField] private Slider bossHealthBar;
        [SerializeField] private RectTransform powerUpRow;

        private Font _font;
        private readonly List<Image> _livesIcons = new List<Image>();

        // Power-up slot bookkeeping.
        private class PowerSlot
        {
            public GameObject root;
            public Image icon;
            public Image fill;
            public float remaining;
            public float total;
        }
        private readonly Dictionary<PowerUpType, PowerSlot> _powerSlots = new Dictionary<PowerUpType, PowerSlot>();

        private PlayerHealth _playerHealth;
        private Bomb _bomb;
        private ScoreManager _score;
        private WaveManager _waveManager;
        private EnemyBoss _boss;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildIfNeeded();
        }

        private void OnEnable()
        {
            PowerUpEvents.OnCollected += HandlePowerUpCollected;
        }

        private void OnDisable()
        {
            PowerUpEvents.OnCollected -= HandlePowerUpCollected;
            Unsubscribe();
        }

        private void Start()
        {
            HookUpReferences();
        }

        // -----------------------------------------------------------------
        // Wiring
        // -----------------------------------------------------------------
        private void HookUpReferences()
        {
            _score = ScoreManager.Instance;
            if (_score != null)
            {
                _score.OnScoreChanged += HandleScoreChanged;
                _score.OnMultiplierChanged += HandleMultiplierChanged;
                HandleScoreChanged(_score.Score);
                HandleMultiplierChanged(_score.Multiplier);
            }

            var playerGo = GameObject.FindGameObjectWithTag(Constants.TagPlayer);
            if (playerGo != null)
            {
                _playerHealth = playerGo.GetComponent<PlayerHealth>();
                _bomb = playerGo.GetComponent<Bomb>();
                if (_playerHealth != null)
                {
                    _playerHealth.OnLivesChanged += HandleLivesChanged;
                    HandleLivesChanged(_playerHealth.Lives);
                }
                if (_bomb != null)
                {
                    _bomb.OnBombCountChanged += HandleBombChanged;
                    HandleBombChanged(_bomb.BombCount);
                }
            }

            _waveManager = FindObjectOfType<WaveManager>();
            if (_waveManager != null)
            {
                _waveManager.OnWaveStart += HandleWaveStart;
                _waveManager.OnBossSpawn += HandleBossSpawn;
            }
        }

        private void Unsubscribe()
        {
            if (_score != null)
            {
                _score.OnScoreChanged -= HandleScoreChanged;
                _score.OnMultiplierChanged -= HandleMultiplierChanged;
            }
            if (_playerHealth != null)
                _playerHealth.OnLivesChanged -= HandleLivesChanged;
            if (_bomb != null)
                _bomb.OnBombCountChanged -= HandleBombChanged;
            if (_waveManager != null)
            {
                _waveManager.OnWaveStart -= HandleWaveStart;
                _waveManager.OnBossSpawn -= HandleBossSpawn;
            }
            if (_boss != null)
                _boss.OnBossHealthChanged -= HandleBossHealth;
        }

        // -----------------------------------------------------------------
        // Event handlers
        // -----------------------------------------------------------------
        private void HandleScoreChanged(int value)
        {
            if (scoreText != null) scoreText.text = "SCORE  " + value.ToString("N0");
        }

        private void HandleMultiplierChanged(float value)
        {
            if (multiplierText != null)
                multiplierText.text = "x" + value.ToString("0.0");
        }

        private void HandleLivesChanged(int lives)
        {
            for (int i = 0; i < _livesIcons.Count; i++)
                _livesIcons[i].enabled = i < lives;

            if (healthBar != null)
            {
                healthBar.maxValue = _playerHealth != null ? _playerHealth.MaxLives : 5;
                healthBar.value = lives;
            }
        }

        private void HandleBombChanged(int count)
        {
            if (bombText != null) bombText.text = "BOMBS  " + count;
        }

        private void HandleWaveStart(int wave)
        {
            if (waveText != null) waveText.text = "WAVE  " + wave;
        }

        private void HandleBossSpawn()
        {
            // Find the boss once it exists (spawn may be slightly delayed).
            StartCoroutine(WaitForBoss());
        }

        private System.Collections.IEnumerator WaitForBoss()
        {
            float timeout = 6f;
            while (EnemyBoss.ActiveBoss == null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            _boss = EnemyBoss.ActiveBoss;
            if (_boss != null)
            {
                if (bossHealthBar != null)
                {
                    bossHealthBar.gameObject.SetActive(true);
                    bossHealthBar.value = 1f;
                }
                _boss.OnBossHealthChanged += HandleBossHealth;
                _boss.OnBossDefeated += HandleBossDefeated;
            }
        }

        private void HandleBossHealth(float normalised)
        {
            if (bossHealthBar != null)
                bossHealthBar.value = normalised;
        }

        private void HandleBossDefeated()
        {
            if (bossHealthBar != null)
                bossHealthBar.gameObject.SetActive(false);
            if (_boss != null)
            {
                _boss.OnBossHealthChanged -= HandleBossHealth;
                _boss.OnBossDefeated -= HandleBossDefeated;
                _boss = null;
            }
        }

        private void HandlePowerUpCollected(PowerUpType type, float duration)
        {
            if (_powerSlots.TryGetValue(type, out var slot))
            {
                slot.total = Mathf.Max(0.01f, duration);
                slot.remaining = duration;
                slot.root.SetActive(true);
            }
        }

        private void Update()
        {
            // Tick power-up timers.
            foreach (var pair in _powerSlots)
            {
                var slot = pair.Value;
                if (!slot.root.activeSelf) continue;
                slot.remaining -= Time.deltaTime;
                if (slot.remaining <= 0f)
                {
                    slot.remaining = 0f;
                    slot.root.SetActive(false);
                }
                if (slot.fill != null)
                    slot.fill.fillAmount = slot.total > 0f ? slot.remaining / slot.total : 0f;
            }
        }

        // -----------------------------------------------------------------
        // Runtime UI construction
        // -----------------------------------------------------------------
        private void BuildIfNeeded()
        {
            // If a score text was assigned, assume the whole HUD is authored.
            if (scoreText != null) return;

            var canvasGo = new GameObject("HUDCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();

            scoreText = CreateText(canvasGo.transform, "ScoreText", "SCORE  0",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -20f), TextAnchor.UpperLeft, 34);
            multiplierText = CreateText(canvasGo.transform, "MultiplierText", "x1.0",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -60f), TextAnchor.UpperLeft, 26);
            waveText = CreateText(canvasGo.transform, "WaveText", "WAVE  1",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperRight, 30);
            bombText = CreateText(canvasGo.transform, "BombText", "BOMBS  1",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -58f), TextAnchor.UpperRight, 24);

            BuildLivesIcons(canvasGo.transform);
            BuildHealthBar(canvasGo.transform);
            BuildBossHealthBar(canvasGo.transform);
            BuildPowerUpRow(canvasGo.transform);
        }

        private Text CreateText(Transform parent, string name, string content,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, TextAnchor align, int fontSize)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var rt = text.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(anchorMin.x, anchorMax.y);
            rt.sizeDelta = new Vector2(400f, 50f);
            rt.anchoredPosition = anchoredPos;
            return text;
        }

        private void BuildLivesIcons(Transform parent)
        {
            var sprite = SpriteGenerator.CreatePlayerSprite();
            for (int i = 0; i < 5; i++)
            {
                var go = new GameObject("Life" + i);
                go.transform.SetParent(parent, false);
                var img = go.AddComponent<Image>();
                img.sprite = sprite;
                img.preserveAspect = true;
                var rt = img.rectTransform;
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.sizeDelta = new Vector2(36f, 36f);
                rt.anchoredPosition = new Vector2(20f + i * 42f, -96f);
                _livesIcons.Add(img);
            }
        }

        private void BuildHealthBar(Transform parent)
        {
            healthBar = CreateSlider(parent, "HealthBar",
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -140f),
                new Vector2(260f, 18f), new Color(0.3f, 0.9f, 0.4f));
            healthBar.minValue = 0;
            healthBar.maxValue = 5;
            healthBar.value = 3;
        }

        private void BuildBossHealthBar(Transform parent)
        {
            bossHealthBar = CreateSlider(parent, "BossHealthBar",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f),
                new Vector2(600f, 22f), new Color(0.95f, 0.3f, 0.3f));
            bossHealthBar.minValue = 0;
            bossHealthBar.maxValue = 1;
            bossHealthBar.value = 1;
            bossHealthBar.gameObject.SetActive(false);
        }

        private Slider CreateSlider(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color fillColour)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var slider = go.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.interactable = false;

            // Background.
            var bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            var bgImg = bg.AddComponent<Image>();
            bgImg.sprite = SpriteGenerator.CreateSquareSprite();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
            StretchFull(bgImg.rectTransform);

            // Fill area / fill.
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            var fillAreaRt = fillArea.AddComponent<RectTransform>();
            StretchFull(fillAreaRt);

            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillImg = fill.AddComponent<Image>();
            fillImg.sprite = SpriteGenerator.CreateSquareSprite();
            fillImg.color = fillColour;
            var fillRt = fillImg.rectTransform;
            fillRt.anchorMin = new Vector2(0f, 0f);
            fillRt.anchorMax = new Vector2(1f, 1f);
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            slider.fillRect = fillRt;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void BuildPowerUpRow(Transform parent)
        {
            var rowGo = new GameObject("PowerUpRow");
            rowGo.transform.SetParent(parent, false);
            powerUpRow = rowGo.AddComponent<RectTransform>();
            powerUpRow.anchorMin = new Vector2(0.5f, 0f);
            powerUpRow.anchorMax = new Vector2(0.5f, 0f);
            powerUpRow.pivot = new Vector2(0.5f, 0f);
            powerUpRow.anchoredPosition = new Vector2(0f, 24f);
            powerUpRow.sizeDelta = new Vector2(5 * 76f, 72f);

            PowerUpType[] order = { PowerUpType.Shield, PowerUpType.RapidFire, PowerUpType.TripleShot, PowerUpType.Bomb, PowerUpType.Speed };
            for (int i = 0; i < order.Length; i++)
                _powerSlots[order[i]] = BuildPowerSlot(powerUpRow, order[i], i, order.Length);
        }

        private PowerSlot BuildPowerSlot(Transform parent, PowerUpType type, int index, int total)
        {
            var slotGo = new GameObject("Slot_" + type);
            slotGo.transform.SetParent(parent, false);
            var slotRt = slotGo.AddComponent<RectTransform>();
            slotRt.sizeDelta = new Vector2(64f, 64f);
            float spacing = 76f;
            float startX = -(total - 1) * spacing * 0.5f;
            slotRt.anchoredPosition = new Vector2(startX + index * spacing, 0f);

            var icon = slotGo.AddComponent<Image>();
            icon.sprite = SpriteGenerator.CreatePowerUpSprite(type);
            icon.preserveAspect = true;

            // Radial fill overlay for the timer.
            var fillGo = new GameObject("Timer");
            fillGo.transform.SetParent(slotGo.transform, false);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = SpriteGenerator.CreateSquareSprite();
            fillImg.color = new Color(0f, 0f, 0f, 0.45f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Vertical;
            fillImg.fillOrigin = (int)Image.OriginVertical.Top;
            fillImg.fillAmount = 1f;
            StretchFull(fillImg.rectTransform);

            var slot = new PowerSlot { root = slotGo, icon = icon, fill = fillImg, remaining = 0f, total = 1f };
            slotGo.SetActive(false);
            return slot;
        }
    }
}
