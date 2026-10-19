using UnityEngine;

namespace MinimalSpaceShooter
{
    public sealed class Projectile : MonoBehaviour
    {
        private const float Speed = 12f;
        private SpaceShooter game;
        private Rigidbody2D body;
        private bool spent;

        public void Initialize(SpaceShooter owner)
        {
            game = owner;
            body = GetComponent<Rigidbody2D>();
            body.velocity = Vector2.up * Speed;
        }

        private void FixedUpdate()
        {
            if (game.Running && body.position.y > SpaceShooter.HalfHeight + 1f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!game.Running || spent) return;
            EnemyShip enemy = other.GetComponent<EnemyShip>();
            if (enemy != null && enemy.TryDestroy())
            {
                spent = true;
                GetComponent<Collider2D>().enabled = false;
                body.simulated = false;
                Destroy(gameObject);
            }
        }
    }
}
