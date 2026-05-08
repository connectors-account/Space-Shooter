using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// In-game HUD showing score, health bar, wave number, and weapon level.
/// Builds UI programmatically at runtime.
/// </summary>
public class GameHUD : MonoBehaviour
{
    private Text scoreText;
    private Text waveText;
    private Text healthText;
    private Text weaponText;
    private Image[] healthPips;
    private RectTransform healthBarFill;
    private Canvas canvas;

    private int maxHealthPips = 10;

    private void Start()
    {
        BuildHUD();
        SubscribeEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnWaveChanged += UpdateWave;
        GameManager.Instance.OnPlayerHealthChanged += UpdateHealth;
    }

    private void UnsubscribeEvents()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnWaveChanged -= UpdateWave;
        GameManager.Instance.OnPlayerHealthChanged -= UpdateHealth;
    }

    private void BuildHUD()
    {
        // Canvas
        GameObject canvasObj = new GameObject("HUDCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Score (top-left)
        scoreText = CreateText("ScoreText", canvasObj.transform, "Score: 0",
            new Vector2(20, -20), new Vector2(300, 40), TextAnchor.UpperLeft,
            32, new Color(1f, 1f, 1f));
        scoreText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
        scoreText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
        scoreText.GetComponent<RectTransform>().pivot = new Vector2(0, 1);

        // Wave (top-center)
        waveText = CreateText("WaveText", canvasObj.transform, "Wave: 1",
            new Vector2(0, -20), new Vector2(300, 40), TextAnchor.UpperCenter,
            32, new Color(1f, 0.9f, 0.3f));
        waveText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 1);
        waveText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 1);
        waveText.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);

        // Health (top-right area)
        healthText = CreateText("HealthText", canvasObj.transform, "HP: 5/5",
            new Vector2(-20, -20), new Vector2(200, 40), TextAnchor.UpperRight,
            28, new Color(0.3f, 1f, 0.3f));
        healthText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
        healthText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        healthText.GetComponent<RectTransform>().pivot = new Vector2(1, 1);

        // Health bar background
        GameObject healthBarBg = CreateUIElement("HealthBarBg", canvasObj.transform);
        Image bgImg = healthBarBg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0f, 0f, 0.7f);
        RectTransform bgRect = healthBarBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(1, 1);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.pivot = new Vector2(1, 1);
        bgRect.anchoredPosition = new Vector2(-20, -55);
        bgRect.sizeDelta = new Vector2(200, 15);

        // Health bar fill
        GameObject healthBarFillObj = CreateUIElement("HealthBarFill", healthBarBg.transform);
        Image fillImg = healthBarFillObj.AddComponent<Image>();
        fillImg.color = new Color(0f, 1f, 0.3f, 0.9f);
        healthBarFill = healthBarFillObj.GetComponent<RectTransform>();
        healthBarFill.anchorMin = Vector2.zero;
        healthBarFill.anchorMax = new Vector2(1, 1);
        healthBarFill.sizeDelta = Vector2.zero;
        healthBarFill.pivot = new Vector2(0, 0.5f);

        // Weapon level (bottom-left)
        weaponText = CreateText("WeaponText", canvasObj.transform, "Weapon: Lv.1",
            new Vector2(20, 20), new Vector2(250, 30), TextAnchor.LowerLeft,
            22, new Color(1f, 0.5f, 0f));
        weaponText.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        weaponText.GetComponent<RectTransform>().anchorMax = new Vector2(0, 0);
        weaponText.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:N0}";
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null)
            waveText.text = $"Wave: {wave}";
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"HP: {current}/{max}";

        if (healthBarFill != null)
        {
            float fill = (float)current / max;
            healthBarFill.anchorMax = new Vector2(fill, 1);

            // Color gradient: green -> yellow -> red
            Image img = healthBarFill.GetComponent<Image>();
            if (img != null)
            {
                if (fill > 0.6f) img.color = new Color(0f, 1f, 0.3f, 0.9f);
                else if (fill > 0.3f) img.color = new Color(1f, 0.8f, 0f, 0.9f);
                else img.color = new Color(1f, 0.2f, 0f, 0.9f);
            }
        }
    }

    private void Update()
    {
        // Update weapon level display
        GameObject player = GameObject.FindGameObjectWithTag(Tags.Player);
        if (player != null && weaponText != null)
        {
            PlayerShooting ps = player.GetComponent<PlayerShooting>();
            if (ps != null)
                weaponText.text = $"Weapon: Lv.{ps.GetWeaponLevel()}";
        }
    }

    // --- Helpers ---

    private Text CreateText(string name, Transform parent, string content,
                           Vector2 position, Vector2 size, TextAnchor anchor,
                           int fontSize, Color color)
    {
        GameObject obj = CreateUIElement(name, parent);
        Text text = obj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = color;

        // Add shadow for readability
        Shadow shadow = obj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(2, -2);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return text;
    }

    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }
}
