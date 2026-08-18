using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Instantly destroys every enemy on screen, awarding 50 points per enemy.
    /// Bypasses the normal per-enemy score value so the bomb bonus is deterministic.
    /// </summary>
    public class PowerUpBomb : PowerUpBase
    {
        [Tooltip("Points awarded per enemy cleared by the bomb.")]
        public int pointsPerEnemy = 50;

        public PowerUpBomb()
        {
            duration = 0f;
        }

        public override void Apply(GameObject player)
        {
            EnemyHealth[] enemies = Object.FindObjectsOfType<EnemyHealth>();
            foreach (EnemyHealth enemy in enemies)
            {
                if (enemy == null || enemy.IsDead) continue;

                if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(pointsPerEnemy);
                if (WaveManager.Instance != null) WaveManager.Instance.NotifyEnemyKilled();
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.explosionSFX);

                Destroy(enemy.gameObject);
            }
        }

        public override void Expire(GameObject player)
        {
            // Instant effect; nothing to reverse.
        }
    }
}
