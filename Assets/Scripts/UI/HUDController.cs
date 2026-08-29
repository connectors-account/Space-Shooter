using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Manages the in-game HUD: score, lives icons, health bar, wave text, boss health bar,
    /// transient messages and floating score popups.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Score & Wave")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text waveText;

        [Header("Lives")]
        [Tooltip("Heart / life icon GameObjects, toggled on/off to reflect remaining lives.")]
        [SerializeField] private GameObject[] lifeIcons;

        [Header("Health")]
        [SerializeField] private Slider healthBar;

        [Header("Boss Health")]
        [SerializeField] private Slider bossHealthBar;
        [SerializeField] private CanvasGroup bossHealthGroup;
        [SerializeField] private float bossFadeSpeed = 4f;

        [Header("Message")]
        [SerializeField] private Text messageText;

        [Header("Score Popup")]
        [SerializeField] private GameObject scorePopupPrefab;
        [SerializeField] private Canvas worldCanvas;
        [SerializeField] private float popupFloatDistance = 60f;
        [SerializeField] private float popupDuration = 1f;

        private Coroutine _messageRoutine;
        private Coroutine _bossFadeRoutine;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;

            if (bossHealthGroup != null)
            {
                bossHealthGroup.alpha = 0f;
                bossHealthGroup.gameObject.SetActive(false);
            }
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (GameManager.HasInstance)
            {
                var gm = GameManager.Instance;
                gm.OnScoreChanged += SetScore;
                gm.OnLivesChanged += SetLives;
                gm.OnWaveChanged += SetWave;

                // Initialize immediately.
                SetScore(gm.Score);
                SetLives(gm.Lives);
                SetWave(gm.CurrentWave);
            }
        }

        private void OnDisable()
        {
            if (GameManager.HasInstance)
            {
                var gm = GameManager.Instance;
                gm.OnScoreChanged -= SetScore;
                gm.OnLivesChanged -= SetLives;
                gm.OnWaveChanged -= SetWave;
            }
        }

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {score:N0}";
            }
        }

        public void SetWave(int wave)
        {
            if (waveText != null)
            {
                waveText.text = $"WAVE {wave}";
            }
        }

        public void SetLives(int lives)
        {
            if (lifeIcons == null)
            {
                return;
            }
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].SetActive(i < lives);
                }
            }
        }

        public void SetHealth(int current, int max)
        {
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
            }
        }

        // ------------------------------------------------------------------
        // Boss health bar
        // ------------------------------------------------------------------
        public void ShowBossHealthBar(float percent)
        {
            if (bossHealthBar != null)
            {
                bossHealthBar.value = Mathf.Clamp01(percent);
            }

            if (bossHealthGroup != null)
            {
                if (!bossHealthGroup.gameObject.activeSelf)
                {
                    bossHealthGroup.gameObject.SetActive(true);
                }
                StartBossFade(1f);
            }
        }

        public void HideBossHealthBar()
        {
            StartBossFade(0f);
        }

        private void StartBossFade(float target)
        {
            if (bossHealthGroup == null)
            {
                return;
            }
            if (_bossFadeRoutine != null)
            {
                StopCoroutine(_bossFadeRoutine);
            }
            _bossFadeRoutine = StartCoroutine(BossFadeRoutine(target));
        }

        private IEnumerator BossFadeRoutine(float target)
        {
            while (!Mathf.Approximately(bossHealthGroup.alpha, target))
            {
                bossHealthGroup.alpha = Mathf.MoveTowards(bossHealthGroup.alpha, target, bossFadeSpeed * Time.deltaTime);
                yield return null;
            }
            bossHealthGroup.alpha = target;
            if (Mathf.Approximately(target, 0f))
            {
                bossHealthGroup.gameObject.SetActive(false);
            }
            _bossFadeRoutine = null;
        }

        // ------------------------------------------------------------------
        // Messages
        // ------------------------------------------------------------------
        public void ShowMessage(string message, float duration)
        {
            if (messageText == null)
            {
                return;
            }
            if (_messageRoutine != null)
            {
                StopCoroutine(_messageRoutine);
            }
            _messageRoutine = StartCoroutine(MessageRoutine(message, duration));
        }

        private IEnumerator MessageRoutine(string message, float duration)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);

            Color baseColor = messageText.color;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = elapsed < duration * 0.75f ? 1f : Mathf.Lerp(1f, 0f, (elapsed - duration * 0.75f) / (duration * 0.25f));
                messageText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                yield return null;
            }

            messageText.color = baseColor;
            messageText.gameObject.SetActive(false);
            _messageRoutine = null;
        }

        // ------------------------------------------------------------------
        // Floating score popup
        // ------------------------------------------------------------------
        public void ShowScorePopup(int amount, Vector3 worldPosition)
        {
            if (scorePopupPrefab == null || worldCanvas == null)
            {
                return;
            }
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            GameObject popup = Instantiate(scorePopupPrefab, worldCanvas.transform);
            Vector2 screenPos = _camera.WorldToScreenPoint(worldPosition);

            RectTransform canvasRect = worldCanvas.transform as RectTransform;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                worldCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _camera,
                out localPoint);

            RectTransform popupRect = popup.transform as RectTransform;
            if (popupRect != null)
            {
                popupRect.anchoredPosition = localPoint;
            }

            Text popupText = popup.GetComponentInChildren<Text>();
            if (popupText != null)
            {
                popupText.text = $"+{amount}";
            }

            StartCoroutine(PopupRoutine(popup, popupRect, popupText));
        }

        private IEnumerator PopupRoutine(GameObject popup, RectTransform rect, Text text)
        {
            float elapsed = 0f;
            Vector2 startPos = rect != null ? rect.anchoredPosition : Vector2.zero;
            Color baseColor = text != null ? text.color : Color.white;

            while (elapsed < popupDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / popupDuration;

                if (rect != null)
                {
                    rect.anchoredPosition = startPos + Vector2.up * (popupFloatDistance * t);
                }
                if (text != null)
                {
                    text.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - t);
                }
                yield return null;
            }

            Destroy(popup);
        }
    }
}
