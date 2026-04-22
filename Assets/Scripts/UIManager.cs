using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD Root")]
    [SerializeField] private GameObject hudRoot;

    [Header("HUD Widgets")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text powerUpText;
    [SerializeField] private Text messageText;

    private Coroutine messageRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetHudVisible(bool visible)
    {
        if (hudRoot != null)
        {
            hudRoot.SetActive(visible);
        }
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    public void UpdateLives(int currentLives, int maxLives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives}/{maxLives}";
        }
    }

    public void UpdateWave(int wave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {wave}/{totalWaves}";
        }
    }

    public void UpdatePowerUpStatus(bool shieldActive, bool rapidFireActive)
    {
        if (powerUpText == null)
        {
            return;
        }

        string shield = shieldActive ? "Shield ON" : "Shield OFF";
        string rapid = rapidFireActive ? "Rapid Fire ON" : "Rapid Fire OFF";
        powerUpText.text = $"{shield} | {rapid}";
    }

    public void ShowMessage(string message, float duration)
    {
        if (messageText == null)
        {
            return;
        }

        if (messageRoutine != null)
        {
            StopCoroutine(messageRoutine);
        }

        messageRoutine = StartCoroutine(MessageRoutine(message, duration));
    }

    private IEnumerator MessageRoutine(string message, float duration)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }
}
