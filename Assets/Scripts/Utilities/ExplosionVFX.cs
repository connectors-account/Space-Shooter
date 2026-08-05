using System.Collections;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Simple pooled explosion effect: scales up and fades out an explosion
    /// sprite, then returns itself to the object pool. Requires no particle
    /// system or art assets.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ExplosionVFX : MonoBehaviour
    {
        [SerializeField] private float duration = 0.4f;
        [SerializeField] private float startScale = 0.3f;
        [SerializeField] private float endScale = 1.4f;

        private SpriteRenderer _sr;
        private Coroutine _routine;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr.sprite == null)
                _sr.sprite = SpriteGenerator.CreateExplosionSprite();
            _sr.sortingOrder = 6;
        }

        public void Play()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(PlayRoutine());
        }

        private void OnEnable()
        {
            // Auto-play when acquired from the pool.
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            Play();
        }

        private IEnumerator PlayRoutine()
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                float scale = Mathf.Lerp(startScale, endScale, k);
                transform.localScale = new Vector3(scale, scale, 1f);
                var c = _sr.color;
                c.a = 1f - k;
                _sr.color = c;
                yield return null;
            }

            transform.localScale = Vector3.one;
            var col = _sr.color; col.a = 1f; _sr.color = col;
            _routine = null;

            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Release(Constants.PoolExplosion, gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
