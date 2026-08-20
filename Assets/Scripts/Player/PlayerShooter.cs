using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Projectiles;

namespace SpaceShooter.Player
{
    public enum ShootMode { Single, Double, Triple }

    /// <summary>
    /// Player weapon controller. Fires on Space held or mouse button, respects a fire-rate
    /// cooldown, and supports Single/Double/Triple shot plus a rapid-fire multiplier.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Firing")]
        public float baseFireRate = 0.2f;    // seconds between shots
        public float bulletSpeed = 14f;
        public int bulletDamage = 10;
        public Transform firePoint;           // spawn origin; defaults to slightly above the ship
        public GameObject bulletPrefab;       // fallback if pools not registered

        [Header("Modes")]
        public ShootMode mode = ShootMode.Single;
        public float tripleShotAngle = 15f;
        public float doubleShotSpacing = 0.25f;

        private float _rapidFireMultiplier = 1f; // <1 = faster
        private float _nextFireTime;

        private void Awake()
        {
            if (firePoint == null)
            {
                var fp = new GameObject("FirePoint").transform;
                fp.SetParent(transform);
                fp.localPosition = new Vector3(0f, 0.5f, 0f);
                firePoint = fp;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            bool firing = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (firing && Time.time >= _nextFireTime)
            {
                Fire();
                _nextFireTime = Time.time + baseFireRate * _rapidFireMultiplier;
            }
        }

        private void Fire()
        {
            Vector2 origin = firePoint.position;
            Vector2 up = Vector2.up;

            switch (mode)
            {
                case ShootMode.Single:
                    BulletPattern.FireSingle(origin, up, bulletPrefab, bulletSpeed, bulletDamage, true);
                    break;

                case ShootMode.Double:
                    BulletPattern.FireSingle(origin + Vector2.left * doubleShotSpacing, up, bulletPrefab, bulletSpeed, bulletDamage, true);
                    BulletPattern.FireSingle(origin + Vector2.right * doubleShotSpacing, up, bulletPrefab, bulletSpeed, bulletDamage, true);
                    break;

                case ShootMode.Triple:
                    FireAtAngle(origin, -tripleShotAngle);
                    FireAtAngle(origin, 0f);
                    FireAtAngle(origin, tripleShotAngle);
                    break;
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("shoot");
        }

        private void FireAtAngle(Vector2 origin, float angleDeg)
        {
            float rad = (90f + angleDeg) * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            BulletPattern.FireSingle(origin, dir, bulletPrefab, bulletSpeed, bulletDamage, true);
        }

        // --- Power-up hooks -------------------------------------------------

        /// <summary>Enable rapid fire (halves the interval). Called by RapidFirePowerUp.</summary>
        public void SetRapidFire(bool active)
        {
            _rapidFireMultiplier = active ? 0.5f : 1f;
        }

        /// <summary>Enable triple shot. Called by TripleShotPowerUp.</summary>
        public void SetTripleShot(bool active)
        {
            mode = active ? ShootMode.Triple : ShootMode.Single;
        }

        public void SetMode(ShootMode newMode)
        {
            mode = newMode;
        }
    }
}
