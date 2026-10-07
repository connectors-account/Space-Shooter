using UnityEngine;

namespace StellarShooter
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour
    {
        private GameSession game;
        private bool friendly, consumed;
        private float expires;
        public void Launch(GameSession session, Vector2 direction, float speed, bool fromPlayer)
        {
            game = session;
            friendly = fromPlayer;
            expires = Time.time + 7;
            GetComponent<Rigidbody2D>().linearVelocity = direction.normalized * speed;
            // Both bullet artworks are lengthwise along their local Y axis.
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - (fromPlayer ? 90 : -90));
        }
        private void Update()
        {
            if (game == null || !game.IsPlaying) return;
            Vector3 p = transform.position;
            if (Time.time > expires || Mathf.Abs(p.x) > 10 || Mathf.Abs(p.y) > 6.5f) Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || game == null || !game.IsPlaying) return;
            if (friendly)
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy == null) return;
                consumed = true;
                enemy.TakeDamage(1);
            }
            else
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null) return;
                consumed = true;
                player.TakeDamage(1);
            }
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject);
        }
    }
}
