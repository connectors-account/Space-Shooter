using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Visual
{
    public class EffectPulse : MonoBehaviour
    {
        private ObjectPoolManager _pool;
        private float _life;
        private float _maxLife;
        private SpriteRenderer _renderer;

        public void Initialize(ObjectPoolManager pool, Color color, float life)
        {
            _pool = pool;
            _renderer ??= GetComponent<SpriteRenderer>();
            _renderer.color = color;
            _life = life;
            _maxLife = life;
            transform.localScale = Vector3.one * 0.25f;
        }

        private void Update()
        {
            _life -= Time.deltaTime;
            var t = 1f - Mathf.Clamp01(_life / _maxLife);
            transform.localScale = Vector3.one * Mathf.Lerp(0.25f, 1.4f, t);

            var color = _renderer.color;
            color.a = Mathf.Lerp(0.9f, 0f, t);
            _renderer.color = color;

            if (_life <= 0f)
            {
                _pool.Release(gameObject);
            }
        }
    }

    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }

        private ObjectPoolManager _pool;

        private void Awake() => Instance = this;

        public void Initialize(ObjectPoolManager pool)
        {
            _pool = pool;
        }

        public void SpawnHit(Vector3 position)
        {
            var fx = _pool.Get("fx_hit", position, Quaternion.identity);
            if (fx != null)
                fx.GetComponent<EffectPulse>().Initialize(_pool, new Color(1f, 0.9f, 0.4f), 0.18f);
        }

        public void SpawnExplosion(Vector3 position)
        {
            var fx = _pool.Get("fx_explosion", position, Quaternion.identity);
            if (fx != null)
                fx.GetComponent<EffectPulse>().Initialize(_pool, new Color(1f, 0.4f, 0.2f), 0.35f);
        }
    }
}
