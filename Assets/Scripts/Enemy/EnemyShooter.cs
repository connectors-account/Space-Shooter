using UnityEngine;
using SpaceShooter.Bullets;
using SpaceShooter.Core;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Fires enemy bullet patterns at randomized intervals (1.5s–3s).
    /// Supports Single, Spread3, Spread5, Aimed and Spiral patterns.
    /// </summary>
    public class EnemyShooter : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float minInterval = 1.5f;
        [SerializeField] private float maxInterval = 3f;

        [Header("Bullet")]
        [SerializeField] private PatternType pattern = PatternType.Single;
        [SerializeField] private float bulletSpeed = 6f;
        [SerializeField] private int bulletDamage = 10;

        [Header("Spiral")]
        [SerializeField] private float spiralStep = 18f;

        [SerializeField] private Transform firePoint;

        private float _timer;
        private float _spiralAngle;
        private Transform _player;

        public void SetPattern(PatternType newPattern) => pattern = newPattern;

        private void OnEnable()
        {
            ResetTimer();
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            _player = playerObj != null ? playerObj.transform : null;
            if (firePoint == null) firePoint = transform;
        }

        private void ResetTimer()
        {
            _timer = Random.Range(minInterval, maxInterval);
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Shoot();
                if (pattern == PatternType.Spiral)
                {
                    // Fire spiral in a rapid burst for a fuller spread.
                    _timer = 0.12f;
                }
                else
                {
                    ResetTimer();
                }
            }
        }

        private void Shoot()
        {
            Vector3 origin = firePoint.position;
            Vector3 targetPos = _player != null ? _player.position : origin + Vector3.down * 5f;

            BulletPattern.Fire(pattern, origin, Vector2.down, targetPos, bulletSpeed, bulletDamage, _spiralAngle);

            if (pattern == PatternType.Spiral)
            {
                _spiralAngle += spiralStep;
                if (_spiralAngle >= 360f) _spiralAngle -= 360f;
            }
        }
    }
}
