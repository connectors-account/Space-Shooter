using SpaceShooter.Enemy;
using SpaceShooter.Player;
using SpaceShooter.Utils;
using UnityEngine;

namespace SpaceShooter.Combat
{
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        public Vector2 Direction { get; private set; }
        public float Speed { get; private set; }
        public int Damage { get; private set; }
        public bool IsPlayerBullet { get; private set; }

        private const float MaxLifetime = 5f;

        public void Initialize(Vector2 direction, float speed, int damage, bool isPlayerBullet, Color color)
        {
            Direction = direction.normalized;
            Speed = speed;
            Damage = damage;
            IsPlayerBullet = isPlayerBullet;

            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteFactory.GetSprite(color, ShapeType.Square, 12);

            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.2f;

            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;

            Destroy(gameObject, MaxLifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(Direction * Speed * Time.deltaTime);

            if (Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 8f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayerBullet)
            {
                var enemy = other.GetComponent<EnemyController>();
                if (enemy == null)
                {
                    return;
                }

                enemy.TakeDamage(Damage);
                Destroy(gameObject);
                return;
            }

            var player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                return;
            }

            player.TakeDamage(Damage);
            Destroy(gameObject);
        }
    }
}
