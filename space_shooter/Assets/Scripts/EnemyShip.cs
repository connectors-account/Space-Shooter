using UnityEngine;

namespace MinimalSpaceShooter
{
    public sealed class EnemyShip : MonoBehaviour
    {
        private const float Speed = 2f;
        private SpaceShooter game;
        private Rigidbody2D body;
        private bool destroyed;

        public void Initialize(SpaceShooter owner)
        {
            game = owner;
            body = GetComponent<Rigidbody2D>();
            body.velocity = Vector2.down * Speed;
        }

        private void FixedUpdate()
        {
            if (!game.Running || destroyed) return;
            if (body.position.y + 0.45f < -SpaceShooter.HalfHeight)
                game.EndRound("An enemy got past you.");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!game.Running || destroyed) return;
            if (other.GetComponent<PlayerShip>() != null)
                game.EndRound("Your ship was hit.");
        }

        public bool TryDestroy()
        {
            if (!game.Running || destroyed) return false;
            destroyed = true;
            // Prevent two bullets from awarding two points before deferred destruction.
            GetComponent<Collider2D>().enabled = false;
            body.simulated = false;
            game.AddPoint();
            Destroy(gameObject);
            return true;
        }
    }
}
