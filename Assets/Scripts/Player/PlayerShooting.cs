// ============================================================================
// PlayerShooting.cs — Fires bullets from the player ship
// Supports normal shot, rapid fire, and spread shot power-ups.
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;

namespace SpaceShooter.Player
{
    public class PlayerShooting : MonoBehaviour
    {
        [Header("Normal Fire")]
        [SerializeField] private float fireRate = 0.2f;           // seconds between shots
        [SerializeField] private Transform firePoint;             // spawn position
        [SerializeField] private string bulletPoolTag = "PlayerBullet";

        [Header("Rapid Fire Power-Up")]
        [SerializeField] private float rapidFireRate = 0.08f;
        [SerializeField] private float rapidFireDuration = 5f;

        [Header("Spread Shot Power-Up")]
        [SerializeField] private float spreadAngle = 15f;
        [SerializeField] private float spreadShotDuration = 5f;

        // ---- State ----
        private float _nextFireTime;
        private float _rapidFireTimer;
        private float _spreadShotTimer;
        public bool IsRapidFire => _rapidFireTimer > 0f;
        public bool IsSpreadShot => _spreadShotTimer > 0f;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;

            // Count down power-up timers
            if (_rapidFireTimer > 0f) _rapidFireTimer -= Time.deltaTime;
            if (_spreadShotTimer > 0f) _spreadShotTimer -= Time.deltaTime;

            // Fire on Space or left mouse
            if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= _nextFireTime)
            {
                Fire();
                float rate = IsRapidFire ? rapidFireRate : fireRate;
                _nextFireTime = Time.time + rate;
            }
        }

        // ====================================================================
        // Power-up activation
        // ====================================================================
        public void ActivateRapidFire()
        {
            _rapidFireTimer = rapidFireDuration;
            AudioManager.Instance?.PlaySFX("PowerUp");
        }

        public void ActivateSpreadShot()
        {
            _spreadShotTimer = spreadShotDuration;
            AudioManager.Instance?.PlaySFX("PowerUp");
        }

        // ====================================================================
        // Bullet spawning
        // ====================================================================
        private void Fire()
        {
            if (ObjectPool.Instance == null) return;
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.5f;

            if (IsSpreadShot)
            {
                // Three bullets in a fan pattern
                SpawnBullet(spawnPos, Quaternion.identity);
                SpawnBullet(spawnPos, Quaternion.Euler(0, 0, spreadAngle));
                SpawnBullet(spawnPos, Quaternion.Euler(0, 0, -spreadAngle));
            }
            else
            {
                SpawnBullet(spawnPos, Quaternion.identity);
            }

            AudioManager.Instance?.PlaySFX("PlayerShoot");
        }

        private void SpawnBullet(Vector3 pos, Quaternion rotation)
        {
            ObjectPool.Instance.Get(bulletPoolTag, pos, rotation);
        }
    }
}
