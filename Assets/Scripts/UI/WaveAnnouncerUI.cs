using System.Collections;
using UnityEngine;
using TMPro;
using SpaceShooter.Core;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Displays a large animated wave banner on WaveManager.OnWaveStart:
    /// fades in, holds, then fades out using a CanvasGroup alpha coroutine.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class WaveAnnouncerUI : MonoBehaviour
    {
        #region Inspector Fields
        [SerializeField] private TextMeshProUGUI _announceText;
        [SerializeField] private float _fadeInTime = 0.4f;
        [SerializeField] private float _holdTime = 1.5f;
        [SerializeField] private float _fadeOutTime = 0.4f;
        #endregion

        #region Private
        private CanvasGroup _canvasGroup;
        private Coroutine _routine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
        }

        private void OnEnable()
        {
            WaveManager.OnWaveStart += HandleWaveStart;
        }

        private void OnDisable()
        {
            WaveManager.OnWaveStart -= HandleWaveStart;
        }
        #endregion

        #region Announcement
        private void HandleWaveStart(int waveNumber, string waveName)
        {
            if (_announceText != null)
                _announceText.text = $"WAVE {waveNumber} — {waveName}";

            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(AnnounceRoutine());
        }

        private IEnumerator AnnounceRoutine()
        {
            yield return Fade(0f, 1f, _fadeInTime);
            yield return new WaitForSeconds(_holdTime);
            yield return Fade(1f, 0f, _fadeOutTime);
            _routine = null;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;
            _canvasGroup.alpha = from;
            while (elapsed < duration)
            {
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _canvasGroup.alpha = to;
        }
        #endregion
    }
}
