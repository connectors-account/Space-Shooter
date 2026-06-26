using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Environment
{
    /// <summary>
    /// A pooled particle explosion. Plays a short burst of coloured particles then returns itself
    /// to the pool once finished.
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ExplosionEffect : MonoBehaviour, IPoolable
    {
        private ParticleSystem _particles;
        private float _timer;
        private float _duration;

        private void Awake()
        {
            _particles = GetComponent<ParticleSystem>();
        }

        /// <summary>
        /// Configures the explosion colour and approximate size, then plays it.
        /// </summary>
        /// <param name="color">Base particle colour.</param>
        /// <param name="scale">Relative size multiplier of the burst.</param>
        public void Play(Color color, float scale = 1f)
        {
            var main = _particles.main;
            main.startColor = color;
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f * scale, 0.4f * scale);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2f * scale, 5f * scale);

            _duration = main.duration + main.startLifetime.constantMax;
            _timer = 0f;
            _particles.Clear();
            _particles.Play();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _duration)
            {
                ExplosionManager.Instance?.Release(gameObject);
            }
        }

        /// <inheritdoc />
        public void OnSpawned()
        {
            _timer = 0f;
        }

        /// <inheritdoc />
        public void OnDespawned()
        {
            if (_particles != null)
            {
                _particles.Stop();
                _particles.Clear();
            }
        }
    }
}
