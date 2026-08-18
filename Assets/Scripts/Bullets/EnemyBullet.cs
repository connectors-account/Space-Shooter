using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Enemy-fired projectile. Damages the player on contact.</summary>
    public class EnemyBullet : BulletBase
    {
        private void Awake()
        {
            speed = 6f;
            damage = 1;
            Type = BulletType.Enemy;
        }

        protected override void OnHit(Collider2D other)
        {
            var player = other.GetComponent<PlayerHealth>();
            if (player == null) player = other.GetComponentInParent<PlayerHealth>();
            if (player == null) return;

            player.TakeDamage(damage);

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.hitSFX);

            ReturnToPool();
        }
    }
}
