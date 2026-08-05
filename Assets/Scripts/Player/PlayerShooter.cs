using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;

namespace SpaceShooter.Player
{
    public enum FirePattern
    {
        Single,
        Triple,
        Rapid
    }

    /// <summary>
    /// Handles player weapon fire. Auto-fires while the fire control is held,
    /// spawning bullets from the object pool. Supports rapid-fire and triple
    /// shot power-ups (each time-limited). Bullets spawn from centre / left /
    /// right offsets relative to the ship.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Fire settings")]
        [SerializeField] private float baseFireRate = 0.22f;      // seconds between shots
        [SerializeField] private float rapidFireRate = 0.09f;
        [SerializeField] private float bulletSpeed = 14f;
        [SerializeField] private int bulletDamage = 1;
        [SerializeField] private Color bulletColour = new Color(0.4f, 0.9f, 1f);

        [Header("Spawn offsets")]
        [SerializeField] private Vector2 centreOffset = new Vector2(0f, 0.6f);
        [SerializeField] private Vector2 leftOffset = new Vector2(-0.35f, 0.45f);
        [SerializeField] private Vector2 rightOffset = new Vector2(0.35f, 0.45f);
        [SerializeField] private float spreadAngle = 12f;

        public FirePattern CurrentPattern { get; private set; } = FirePattern.Single;

        private float _fireTimer;
        private PlayerInputHandler _input;

        private float _rapidFireTimer;
        private float _tripleShotTimer;

        private void Awake()
        {
            _input = GetComponent<PlayerInputHandler>();
            if (_input == null) _input = gameObject.AddComponent<PlayerInputHandler>();
        }

        private void Update()
        {
            TickPowerUpTimers();

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            _fireTimer -= Time.deltaTime;

            bool wantFire = _input != null && (_input.FireHeld || _input.FirePressed);
            if (wantFire && _fireTimer <= 0f)
            {
                Shoot();
                _fireTimer = _rapidFireTimer > 0f ? rapidFireRate : baseFireRate;
            }
        }

        private void TickPowerUpTimers()
        {
            if (_rapidFireTimer > 0f)
            {
                _rapidFireTimer -= Time.deltaTime;
                if (_rapidFireTimer <= 0f) RecomputePattern();
            }
            if (_tripleShotTimer > 0f)
            {
                _tripleShotTimer -= Time.deltaTime;
                if (_tripleShotTimer <= 0f) RecomputePattern();
            }
        }

        private void RecomputePattern()
        {
            if (_tripleShotTimer > 0f) CurrentPattern = FirePattern.Triple;
            else if (_rapidFireTimer > 0f) CurrentPattern = FirePattern.Rapid;
            else CurrentPattern = FirePattern.Single;
        }

        /// <summary>Fire one volley according to the current pattern.</summary>
        public void Shoot()
        {
            Vector3 origin = transform.position;

            if (_tripleShotTimer > 0f)
            {
                SpawnBullet(origin + (Vector3)centreOffset, 0f);
                SpawnBullet(origin + (Vector3)leftOffset, spreadAngle);
                SpawnBullet(origin + (Vector3)rightOffset, -spreadAngle);
            }
            else
            {
                SpawnBullet(origin + (Vector3)centreOffset, 0f);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(Constants.SfxPlayerShoot, 0.7f);
        }

        private void SpawnBullet(Vector3 position, float angleDegrees)
        {
            if (ObjectPool.Instance == null) return;

            var go = ObjectPool.Instance.Acquire(Constants.PoolPlayerBullet, position, Quaternion.identity);
            if (go == null) return;

            var bullet = go.GetComponent<Bullet>();
            if (bullet == null) bullet = go.AddComponent<Bullet>();

            Vector2 dir = Quaternion.Euler(0f, 0f, angleDegrees) * Vector2.up;
            bullet.Configure(dir, bulletSpeed, bulletDamage, Constants.TagPlayerBullet, bulletColour);
        }

        // -----------------------------------------------------------------
        // Power-up activators
        // -----------------------------------------------------------------
        public void ActivateRapidFire(float duration)
        {
            _rapidFireTimer = Mathf.Max(_rapidFireTimer, duration);
            RecomputePattern();
        }

        public void ActivateTripleShot(float duration)
        {
            _tripleShotTimer = Mathf.Max(_tripleShotTimer, duration);
            RecomputePattern();
        }

        public float RapidFireRemaining => Mathf.Max(0f, _rapidFireTimer);
        public float TripleShotRemaining => Mathf.Max(0f, _tripleShotTimer);

        public void ResetShooter()
        {
            _rapidFireTimer = 0f;
            _tripleShotTimer = 0f;
            _fireTimer = 0f;
            RecomputePattern();
        }
    }
}
