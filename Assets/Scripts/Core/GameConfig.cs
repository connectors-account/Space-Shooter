using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Centralised, designer-friendly tuning values for the whole game. A single instance is
    /// created by the bootstrap and shared between systems so balancing can be done in one place.
    /// </summary>
    [System.Serializable]
    public class GameConfig
    {
        [Header("Player")]
        public float PlayerMoveSpeed = 8f;
        public int PlayerMaxHealth = 100;
        public int PlayerStartingLives = 3;
        public float PlayerFireCooldown = 0.22f;
        public float PlayerRapidFireCooldown = 0.08f;
        public float PlayerBulletSpeed = 14f;
        public int PlayerBulletDamage = 25;
        public float PlayerInvulnerabilityAfterHit = 1.25f;

        [Header("Power-up durations (seconds)")]
        public float ShieldDuration = 6f;
        public float RapidFireDuration = 8f;
        public float SpreadShotDuration = 8f;
        public float ScoreMultiplierDuration = 10f;
        public int ScoreMultiplierFactor = 2;
        public int HealthPickupAmount = 35;
        public float PowerUpDropChance = 0.18f;
        public float PowerUpFallSpeed = 3f;

        [Header("Enemies")]
        public float EnemyBulletSpeed = 6f;
        public int EnemyContactDamage = 20;

        [Header("Waves")]
        public int TotalWaves = 15;
        public int BossEveryNWaves = 5;
        public float TimeBetweenWaves = 2.5f;
        public int BaseEnemiesPerWave = 5;
        public int EnemiesAddedPerWave = 2;

        [Header("World bounds (world units from centre)")]
        public float PlayWidth = 9f;
        public float PlayHeight = 5f;

        /// <summary>Returns the half-width of the playable area.</summary>
        public float HalfWidth => PlayWidth;

        /// <summary>Returns the half-height of the playable area.</summary>
        public float HalfHeight => PlayHeight;

        /// <summary>Clamps a world position to the playable area for the player ship.</summary>
        /// <param name="position">Desired position.</param>
        /// <param name="padding">Extra inset from the edges, in world units.</param>
        public Vector3 ClampToPlayfield(Vector3 position, float padding = 0.5f)
        {
            position.x = Mathf.Clamp(position.x, -PlayWidth + padding, PlayWidth - padding);
            position.y = Mathf.Clamp(position.y, -PlayHeight + padding, PlayHeight - padding);
            position.z = 0f;
            return position;
        }
    }
}
