using System.Collections;
using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Coroutine-based camera shake. Attach to the main camera (or a camera
    /// holder). Call <see cref="Shake"/> to trigger a brief positional shake.
    /// A static <see cref="Instance"/> makes it easy to invoke from anywhere.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 _originalLocalPos;
        private Coroutine _running;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Keep the first one; a scene may briefly hold two cameras.
                Instance = this;
            }
            else
            {
                Instance = this;
            }
            _originalLocalPos = transform.localPosition;
        }

        private void OnDisable()
        {
            if (_running != null)
            {
                StopCoroutine(_running);
                _running = null;
            }
            transform.localPosition = _originalLocalPos;
        }

        /// <summary>Shake the camera for <paramref name="duration"/> seconds.</summary>
        public void Shake(float duration, float magnitude)
        {
            if (!isActiveAndEnabled) return;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(ShakeRoutine(duration, magnitude));
        }

        /// <summary>Static convenience wrapper – safe to call when no instance exists.</summary>
        public static void ShakeStatic(float duration, float magnitude)
        {
            if (Instance != null) Instance.Shake(duration, magnitude);
        }

        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float damper = 1f - Mathf.Clamp01(elapsed / duration);
                float offsetX = (Random.value * 2f - 1f) * magnitude * damper;
                float offsetY = (Random.value * 2f - 1f) * magnitude * damper;
                transform.localPosition = _originalLocalPos + new Vector3(offsetX, offsetY, 0f);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localPosition = _originalLocalPos;
            _running = null;
        }
    }
}
