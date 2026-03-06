using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages all UI elements including score display, health bar, wave counter, and announcements.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject gameUI;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text highScoreText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text healthText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Text waveAnnouncementText;
    [SerializeField] private GameObject waveAnnouncementPanel;
    
    [Header("Settings")]
    [SerializeField] private float announcementDuration = 2f;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private Color normalHealthColor = Color.green;
    
    private Coroutine announcementCoroutine;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
        
        if (waveAnnouncementPanel != null)
        {
            waveAnnouncementPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show the in-game UI
    /// </summary>
    public void ShowGameUI()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the in-game UI
    /// </summary>
    public void HideGameUI()
    {
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update score display
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {score:N0}";
        }
    }
    
    /// <summary>
    /// Update high score display
    /// </summary>
    public void UpdateHighScore(int highScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"HIGH: {highScore:N0}";
        }
    }
    
    /// <summary>
    /// Update wave display
    /// </summary>
    public void UpdateWave(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"WAVE {wave}";
        }
    }
    
    /// <summary>
    /// Update health display
    /// </summary>
    public void UpdateHealth(int current, int max)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {current}/{max}";
        }
        
        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = current;
            
            // Change color based on health percentage
            float healthPercent = (float)current / max;
            Image fillImage = healthBar.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowHealthColor, normalHealthColor, healthPercent);
            }
        }
    }
    
    /// <summary>
    /// Show wave announcement
    /// </summary>
    public void ShowWaveAnnouncement(int wave)
    {
        if (announcementCoroutine != null)
        {
            StopCoroutine(announcementCoroutine);
        }
        announcementCoroutine = StartCoroutine(ShowAnnouncementCoroutine($"WAVE {wave}"));
    }
    
    /// <summary>
    /// Show custom announcement
    /// </summary>
    public void ShowAnnouncement(string message)
    {
        if (announcementCoroutine != null)
        {
            StopCoroutine(announcementCoroutine);
        }
        announcementCoroutine = StartCoroutine(ShowAnnouncementCoroutine(message));
    }
    
    private IEnumerator ShowAnnouncementCoroutine(string message)
    {
        if (waveAnnouncementPanel != null && waveAnnouncementText != null)
        {
            waveAnnouncementText.text = message;
            waveAnnouncementPanel.SetActive(true);
            
            // Animate scale
            Transform panelTransform = waveAnnouncementPanel.transform;
            float elapsed = 0f;
            
            // Scale in
            while (elapsed < 0.3f)
            {
                float scale = Mathf.Lerp(0f, 1.2f, elapsed / 0.3f);
                panelTransform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Scale to normal
            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                float scale = Mathf.Lerp(1.2f, 1f, elapsed / 0.1f);
                panelTransform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            panelTransform.localScale = Vector3.one;
            
            yield return new WaitForSeconds(announcementDuration);
            
            // Fade out
            elapsed = 0f;
            while (elapsed < 0.3f)
            {
                float scale = Mathf.Lerp(1f, 0f, elapsed / 0.3f);
                panelTransform.localScale = Vector3.one * scale;
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            waveAnnouncementPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Create UI elements programmatically (for setup)
    /// </summary>
    public void CreateUIElements()
    {
        // This method can be used to create UI elements via script
        // In practice, these would be set up in the Unity Editor
        
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // Create Game UI container
        gameUI = CreatePanel("GameUI", canvas.transform);
        
        // Create Score Text
        scoreText = CreateText("ScoreText", gameUI.transform, new Vector2(10, -10), TextAnchor.UpperLeft);
        scoreText.text = "SCORE: 0";
        
        // Create Wave Text
        waveText = CreateText("WaveText", gameUI.transform, new Vector2(0, -10), TextAnchor.UpperCenter);
        waveText.text = "WAVE 1";
        
        // Create Health Text
        healthText = CreateText("HealthText", gameUI.transform, new Vector2(-10, -10), TextAnchor.UpperRight);
        healthText.text = "HP: 100/100";
    }
    
    private GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        
        return panel;
    }
    
    private Text CreateText(string name, Transform parent, Vector2 position, TextAnchor alignment)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(200, 30);
        
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = alignment;
        
        return text;
    }
}
