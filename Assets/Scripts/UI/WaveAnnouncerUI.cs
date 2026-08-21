using System.Collections;
using UnityEngine;
using TMPro;
using SpaceShooter.Enemy;

namespace SpaceShooter.UI
{
    /// <summary>
    /// Announces each wave with text that slides in from the side, holds, then slides out.
    /// Shows "BOSS INCOMING!" on boss waves.
    /// </summary>
    public class WaveAnnouncerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private RectTransform announcementRoot;
        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text subText;

        [Header("Animation")]
        [SerializeField] private float slideDistance = 900f;
        [SerializeField] private float slideDuration = 0.5f;
        [SerializeField] private float holdDuration = 2f;

        [Header("Colors")]
        [SerializeField] private Color normalColor = Color.cyan;
        [SerializeField] private Color bossColor = new Color(1f, 0.4f, 0.1f);

        private Coroutine _routine;

        private void Awake()
        {
            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
        }

        private void Start()
        {
            if (announcementRoot != null)
            {
                Vector2 p = announcementRoot.anchoredPosition;
                p.x = -slideDistance;
                announcementRoot.anchoredPosition = p;
            }

            if (waveManager != null)
            {
                waveManager.OnWaveAnnounced += Announce;
            }
        }

        private void OnDestroy()
        {
            if (waveManager != null)
            {
                waveManager.OnWaveAnnounced -= Announce;
            }
        }

        private void Announce(int waveNumber, bool isBossWave)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
            }
            _routine = StartCoroutine(AnnounceRoutine(waveNumber, isBossWave));
        }

        private IEnumerator AnnounceRoutine(int waveNumber, bool isBossWave)
        {
            if (mainText != null)
            {
                mainText.text = $"WAVE {waveNumber}";
                mainText.color = isBossWave ? bossColor : normalColor;
            }
            if (subText != null)
            {
                subText.gameObject.SetActive(isBossWave);
                if (isBossWave) subText.text = "BOSS INCOMING!";
            }

            if (announcementRoot == null) yield break;

            // Slide in from the left.
            yield return Slide(-slideDistance, 0f);
            // Hold on-screen.
            yield return new WaitForSeconds(holdDuration);
            // Slide out to the right.
            yield return Slide(0f, slideDistance);
            // Reset off-screen left for next time.
            Vector2 reset = announcementRoot.anchoredPosition;
            reset.x = -slideDistance;
            announcementRoot.anchoredPosition = reset;
        }

        private IEnumerator Slide(float fromX, float toX)
        {
            float t = 0f;
            float duration = Mathf.Max(0.01f, slideDuration);
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                Vector2 pos = announcementRoot.anchoredPosition;
                pos.x = Mathf.Lerp(fromX, toX, k);
                announcementRoot.anchoredPosition = pos;
                yield return null;
            }
            Vector2 final = announcementRoot.anchoredPosition;
            final.x = toX;
            announcementRoot.anchoredPosition = final;
        }
    }
}
