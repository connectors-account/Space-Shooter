using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Firing patterns available to the player.</summary>
    public enum FireMode
    {
        Single,
        Triple,
        Spread5
    }

    /// <summary>
    /// Player weapon. Fires on Space / Left-Click, throttled by <see cref="fireRate"/>,
    /// and supports Single, Triple and 5-way spread patterns.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        public FireMode fireMode = FireMode.Single;

        [Tooltip("Minimum seconds between shots.")]
        public float fireRate = 0.2f;

        [Tooltip("Optional muzzle transform. Defaults to slightly above the ship.")]
        [SerializeField] private Transform firePoint;

        [SerializeField] private float muzzleOffset = 0.5f;

        private float _lastFireTime = -999f;

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
            {
                Fire();
            }
        }

        /// <summary>
        /// Attempts to fire according to the current <see cref="fireMode"/>.
        /// Returns the bullets spawned this call (empty list while on cooldown).
        /// </summary>
        public List<BulletBase> Fire()
        {
            var spawned = new List<BulletBase>();
            if (Time.time - _lastFireTime < fireRate) return spawned;
            _lastFireTime = Time.time;

            Vector3 origin = firePoint != null
                ? firePoint.position
                : transform.position + Vector3.up * muzzleOffset;

            if (BulletPool.Instance == null) return spawned;

            foreach (Vector2 dir in GetShotDirections(fireMode))
            {
                BulletBase bullet = BulletPool.Instance.GetBullet(BulletType.Player, origin, dir.normalized);
                if (bullet != null) spawned.Add(bullet);
            }

            if (spawned.Count > 0 && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.shootSFX);
            }

            return spawned;
        }

        /// <summary>
        /// Returns the unit direction vectors for a given fire mode. Angles are measured
        /// from straight up: Single {0}, Triple {-15,0,+15}, Spread5 {-30,-15,0,+15,+30}.
        /// </summary>
        public Vector2[] GetShotDirections(FireMode mode)
        {
            switch (mode)
            {
                case FireMode.Triple:
                    return new[]
                    {
                        RotateFromUp(-15f),
                        RotateFromUp(0f),
                        RotateFromUp(15f)
                    };
                case FireMode.Spread5:
                    return new[]
                    {
                        RotateFromUp(-30f),
                        RotateFromUp(-15f),
                        RotateFromUp(0f),
                        RotateFromUp(15f),
                        RotateFromUp(30f)
                    };
                case FireMode.Single:
                default:
                    return new[] { RotateFromUp(0f) };
            }
        }

        /// <summary>
        /// Returns the unit vector produced by rotating "up" by <paramref name="degrees"/>
        /// such that Vector2.SignedAngle(up, result) == degrees.
        /// </summary>
        private static Vector2 RotateFromUp(float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            return new Vector2(-Mathf.Sin(rad), Mathf.Cos(rad));
        }
    }
}
