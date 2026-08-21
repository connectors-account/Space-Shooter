using System.Collections;
using UnityEngine;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Simple camera shake. Call Shake(duration, magnitude) from gameplay events.
    /// Uses unscaled offsets around the camera's rest position and smoothly returns.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [SerializeField] private float returnSpeed = 8f;

        private Vector3 originalLocalPos;
        private Coroutine shakeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            originalLocalPos = transform.localPosition;
        }

        public void Shake(float duration, float magnitude)
        {
            if (shakeRoutine != null) StopCoroutine(shakeRoutine);
            shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float damper = 1f - (elapsed / duration);
                float offsetX = Random.Range(-1f, 1f) * magnitude * damper;
                float offsetY = Random.Range(-1f, 1f) * magnitude * damper;
                transform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // Smoothly return to origin.
            while (Vector3.Distance(transform.localPosition, originalLocalPos) > 0.001f)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalLocalPos, returnSpeed * Time.unscaledDeltaTime);
                yield return null;
            }
            transform.localPosition = originalLocalPos;
            shakeRoutine = null;
        }
    }
}
