using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class EnemyShip : MonoBehaviour
    {
        public EnemyKind kind;
        public int points = 100;
        private Rigidbody2D body;
        private SpriteRenderer visual;
        private int health;
        private int wave;
        private float age;
        private float baseX;
        private float phase;
        private float nextShot;
        private float flashUntil;
        private bool removed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = GetComponent<SpriteRenderer>();
        }

        public void Configure(int currentWave, int index)
        {
            wave = currentWave;
            health = (kind == EnemyKind.Scout ? 2 : kind == EnemyKind.Gunship ? 5 : 9) + Mathf.Min(wave / 4, 6);
            baseX = transform.position.x;
            phase = index * 1.7f;
            nextShot = 0.8f + (index % 3) * 0.2f;
        }

        private void FixedUpdate()
        {
            GameController game = GameController.Instance;
            if (!game.IsPlaying || removed) return;
            age += Time.fixedDeltaTime;
            float speed = (kind == EnemyKind.Scout ? 2.4f : kind == EnemyKind.Gunship ? 1.55f : 1.1f) + Mathf.Min(wave * 0.06f, 1.6f);
            float x = baseX + Mathf.Sin(age * 1.8f + phase) * (kind == EnemyKind.Scout ? 0.65f : 1.1f);
            x = Mathf.Clamp(x, -game.HalfWidth + 0.95f, game.HalfWidth - 0.95f);
            body.MovePosition(new Vector2(x, body.position.y - speed * Time.fixedDeltaTime));
            visual.color = Time.time < flashUntil ? new Color(1f, 0.5f, 0.5f) : Color.white;
            if (body.position.y < -10.2f) { Remove(false); return; }
            if (age >= nextShot && body.position.y < 8.2f && body.position.y > -6.8f)
            {
                nextShot = age + Mathf.Max(0.7f, 1.9f - wave * 0.04f);
                Shoot();
            }
        }

        private void Shoot()
        {
            GameController game = GameController.Instance;
            if (game.Player == null) return;
            Vector2 origin = body.position + Vector2.down * 0.65f;
            Vector2 aim = ((Vector2)game.Player.transform.position - origin).normalized;
            float speed = 4f + Mathf.Min(wave * 0.12f, 2.5f);
            if (kind == EnemyKind.Scout) game.Fire(false, origin, aim, speed);
            else if (kind == EnemyKind.Gunship)
            {
                for (int i = -1; i <= 1; i++) game.Fire(false, origin, Rotate(aim, i * 19f), speed);
            }
            else
            {
                for (int i = 0; i < 8; i++) game.Fire(false, origin, Rotate(Vector2.down, i * 45f + age * 18f), speed * 0.85f);
            }
        }

        private static Vector2 Rotate(Vector2 vector, float degrees)
        {
            float angle = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(angle), s = Mathf.Sin(angle);
            return new Vector2(vector.x * c - vector.y * s, vector.x * s + vector.y * c);
        }

        public void Damage(int amount)
        {
            if (removed || !GameController.Instance.IsPlaying) return;
            health -= amount;
            flashUntil = Time.time + 0.06f;
            if (health <= 0) Remove(true);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerShip player = other.GetComponent<PlayerShip>();
            if (player == null || removed || !GameController.Instance.IsPlaying) return;
            player.Damage(1);
            // Contact consumes the enemy but earns no kill score or power-up.
            Remove(false, false);
        }

        private void Remove(bool destroyed, bool escapePenalty = true)
        {
            if (removed) return;
            removed = true;
            GetComponent<Collider2D>().enabled = false;
            GameController.Instance.EnemyRemoved(this, destroyed, escapePenalty);
            Destroy(gameObject);
        }
    }
}
