using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Displays wave announcements ("Wave X") in the center of the screen with fade animation.
/// </summary>
public class WaveAnnouncement : MonoBehaviour
{
    private Text waveText;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        BuildUI();

        if (GameManager.Instance != null)
            GameManager.Instance.OnWaveChanged += ShowWaveAnnouncement;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnWaveChanged -= ShowWaveAnnouncement;
    }

    private void BuildUI()
    {
        GameObject canvasObj = new GameObject("WaveAnnouncementCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        GameObject textObj = new GameObject("WaveText", typeof(RectTransform));
        textObj.transform.SetParent(canvasObj.transform, false);
        waveText = textObj.AddComponent<Text>();
        waveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        waveText.fontSize = 52;
        waveText.alignment = TextAnchor.MiddleCenter;
        waveText.color = new Color(1f, 0.9f, 0.3f);

        Shadow shadow = textObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.8f);
        shadow.effectDistance = new Vector2(3, -3);

        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, 50);
        rect.sizeDelta = new Vector2(600, 80);
    }

    private void ShowWaveAnnouncement(int wave)
    {
        string text = (wave % 5 == 0 && wave > 0) ? $"★ BOSS WAVE {wave} ★" : $"Wave {wave}";
        waveText.text = text;

        if (wave % 5 == 0)
            waveText.color = new Color(1f, 0.3f, 0.3f); // Red for boss
        else
            waveText.color = new Color(1f, 0.9f, 0.3f);

        StopAllCoroutines();
        StartCoroutine(AnimateAnnouncement());
    }

    private IEnumerator AnimateAnnouncement()
    {
        // Fade in
        float duration = 0.5f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            canvasGroup.alpha = t / duration;
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(1.5f);

        // Fade out
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            canvasGroup.alpha = 1f - (t / duration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
