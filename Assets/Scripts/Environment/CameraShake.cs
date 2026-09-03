using System.Collections;
using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// Singleton camera shake. Offsets the main camera by a random amount each
    /// frame for a duration, then restores the original position.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        #region Singleton
        public static CameraShake Instance { get; private set; }
        #endregion

        #region Fields
        private Transform _camTransform;
        private Vector3 _originalLocalPos;
        private Coroutine _shakeRoutine;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _camTransform = Camera.main != null ? Camera.main.transform : transform;
            _originalLocalPos = _camTransform.localPosition;
        }
        #endregion

        #region Public API
        /// <summary>Shakes the camera for <paramref name="duration"/> seconds.</summary>
        public void Shake(float duration, float magnitude)
        {
            if (_camTransform == null)
            {
                _camTransform = Camera.main != null ? Camera.main.transform : transform;
                _originalLocalPos = _camTransform.localPosition;
            }

            if (_shakeRoutine != null) StopCoroutine(_shakeRoutine);
            _shakeRoutine = StartCoroutine(ShakeRoutine(duration, magnitude));
        }
        #endregion

        #region Coroutine
        private IEnumerator ShakeRoutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                Vector3 offset = Random.insideUnitSphere * magnitude;
                offset.z = 0f;
                _camTransform.localPosition = _originalLocalPos + offset;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _camTransform.localPosition = _originalLocalPos;
            _shakeRoutine = null;
        }
        #endregion
    }
}
