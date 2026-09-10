using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class EnemyShip : MonoBehaviour
    {
        [Range(0, 2)] public int pattern;
        Rigidbody2D body;
        float age, originX, nextShot, speed, phase;
        int health;
        bool dead;
        void Awake() { body = GetComponent<Rigidbody2D>(); }
        public void Configure(int wave, int index)
        {
            originX = transform.position.x; phase = index * 1.73f;
            health = GameRules.EnemyHealth(wave, pattern);
            speed = Mathf.Min(2.4f, 1.05f + wave * 0.065f);
            nextShot = 1.1f + (index % 3) * 0.25f;
        }
        void FixedUpdate()
        {
            if (StarGame.Instance.State != RunState.Playing || dead) return;
            age += Time.fixedDeltaTime;
            float sway = Mathf.Sin(age * (pattern == 2 ? 2 : 1.3f) + phase) * (pattern == 0 ? 0.7f : 1.35f);
            body.MovePosition(new Vector2(Mathf.Clamp(originX + sway, -8, 8), body.position.y - speed * Time.fixedDeltaTime));
            if (body.position.y < -5.8f) { dead = true; Destroy(gameObject); return; }
            if (age >= nextShot && body.position.y < 4.5f && body.position.y > -3.3f)
            {
                Shoot(); nextShot = age + (pattern == 2 ? 1.75f : 1.45f);
            }
        }
        void Shoot()
        {
            var game = StarGame.Instance;
            if (game.Player == null) return;
            var prefab = game.assets.enemyBullet;
            Vector3 p = transform.position + Vector3.down * 0.45f;
            if (pattern == 0)
            {
                Vector2 aim = ((Vector2)game.Player.transform.position - (Vector2)p).normalized;
                Projectile.Spawn(prefab, p, aim, 3.5f);
            }
            else if (pattern == 1)
            {
                for (int i = -2; i <= 2; i++) Projectile.Spawn(prefab, p, Quaternion.Euler(0, 0, i * 18) * Vector2.down, 3.1f);
            }
            else
            {
                for (int i = 0; i < 8; i++) Projectile.Spawn(prefab, p, Quaternion.Euler(0, 0, i * 45 + age * 23) * Vector2.down, 2.7f);
            }
            game.Sound(game.assets.enemyShoot, 0.07f);
        }
        public void Hit()
        {
            if (dead) return;
            health--;
            if (health > 0) { StarGame.Instance.Burst(transform.position, Color.yellow, 3); return; }
            dead = true;
            GetComponent<Collider2D>().enabled = false;
            StarGame.Instance.EnemyDestroyed(transform.position, 100 * (pattern + 1));
            Destroy(gameObject);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (dead || StarGame.Instance.State != RunState.Playing) return;
            var player = other.GetComponent<PlayerShip>();
            if (player == null) return;
            player.Damage(); dead = true;
            GetComponent<Collider2D>().enabled = false;
            StarGame.Instance.Burst(transform.position, new Color(1, 0.4f, 0.3f), 10);
            Destroy(gameObject); // Ramming never awards score or a drop.
        }
    }
}
