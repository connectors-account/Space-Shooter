using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Player-fired projectile. Damages enemies on contact.</summary>
    public class PlayerBullet : BulletBase
    {
        private void Awake()
        {
            speed = 15f;
            damage = 1;
            Type = BulletType.Player;
        }

        protected override void OnHit(Collider2D other)
        {
            // Ignore other player-owned objects.
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null) enemy = other.GetComponentInParent<EnemyHealth>();
            if (enemy == null) return;

            enemy.TakeDamage(damage);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSFX);

            ReturnToPool();
        }
    }
}
