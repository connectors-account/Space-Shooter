// ============================================================================
// EnemyTracker.cs — Moves toward the player's current X position
// Creates a homing feel but only on the horizontal axis.
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemyTracker : EnemyBase
    {
        [Header("Tracking")]
        [SerializeField] private float trackingStrength = 2f;   // horizontal lerp speed

        private Transform _playerTransform;

        protected override void OnEnable()
        {
            base.OnEnable();
            var player = GameObject.FindGameObjectWithTag("Player");
            _playerTransform = player != null ? player.transform : null;
        }

        protected override void Move()
        {
            // Always move downward
            float newY = transform.position.y - moveSpeed * Time.deltaTime;

            // Track player X with smooth interpolation
            float targetX = _playerTransform != null ? _playerTransform.position.x : transform.position.x;
            float newX = Mathf.MoveTowards(transform.position.x, targetX, trackingStrength * Time.deltaTime);

            transform.position = new Vector3(newX, newY, 0f);
        }
    }
}
