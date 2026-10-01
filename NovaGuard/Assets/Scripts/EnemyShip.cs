using UnityEngine;

namespace NovaGuard
{
    public enum EnemyKind { Scout, Gunner, Tank }

    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class EnemyShip : MonoBehaviour
    {
        public EnemyKind kind;
        public int baseHealth = 2;
        public int points = 100;
        public float speed = 2f;
        public float shotInterval = 2f;
        private int health;
        private float age, originX, nextShot, waveSpeed, phase, flashUntil;
        private bool retired;
        private Rigidbody2D body;
        private SpriteRenderer image;
        private GameController Game => GameController.Instance;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            image = GetComponent<SpriteRenderer>();
        }

        public void Configure(int wave)
        {
            health = baseHealth + Mathf.Min(3, (wave - 1) / 4);
            waveSpeed = 1 + Mathf.Min(0.6f, (wave - 1) * 0.04f);
            originX = transform.position.x;
            phase = Random.value * Mathf.PI * 2;
            nextShot = Time.time + Random.Range(1.1f, 2.3f);
            Game.Register(this);
        }

        private void FixedUpdate()
        {
            if (!Game.IsPlaying || retired) return;
            age += Time.fixedDeltaTime;
            float sway = kind == EnemyKind.Scout ? 1.5f : 0.7f;
            float x = Mathf.Clamp(originX + Mathf.Sin(age * 1.8f + phase) * sway, -14, 14);
            body.MovePosition(new Vector2(x, body.position.y - speed * waveSpeed * Time.fixedDeltaTime));
        }

        private void Update()
        {
            if (!Game.IsPlaying || retired) return;
            image.color = Time.time < flashUntil ? new Color(1, 0.55f, 0.55f) : Color.white;
            if (transform.position.y < -9.5f)
            {
                // Escaped enemies cost hull, so avoiding combat forever is not a winning strategy.
                if (Game.Player != null) Game.Player.TakeDamage(1);
                Retire();
                return;
            }
            if (Time.time < nextShot || transform.position.y > 7 || transform.position.y < -5.5f) return;
            nextShot = Time.time + shotInterval / waveSpeed;
            Shoot();
        }

        private void Shoot()
        {
            if (Game.Player == null) return;
            Vector2 start = (Vector2)transform.position + Vector2.down * 0.65f;
            Vector2 aim = ((Vector2)Game.Player.transform.position - start).normalized;
            if (kind == EnemyKind.Scout) Game.Fire(start, Vector2.down, false, 5.5f);
            else if (kind == EnemyKind.Gunner) Game.Fire(start, aim, false, 6.5f);
            else
            {
                for (int i = -2; i <= 2; i++)
                    Game.Fire(start, Quaternion.Euler(0, 0, i * 16f) * aim, false, 5f);
            }
            Game.sound.Play(SoundCue.EnemyFire);
        }

        public void TakeDamage(int amount)
        {
            if (retired || !Game.IsPlaying) return;
            health -= amount;
            flashUntil = Time.time + 0.08f;
            if (health > 0) return;
            Game.AddScore(points);
            Game.DropPowerUp(transform.position);
            Game.SpawnEffect(transform.position, kind == EnemyKind.Gunner ? Color.magenta : new Color(1, .45f, .1f));
            Game.sound.Play(SoundCue.Explosion);
            Retire();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (retired || !Game.IsPlaying) return;
            PlayerShip player = other.GetComponent<PlayerShip>();
            if (player == null) return;
            player.TakeDamage(1);
            Game.SpawnEffect(transform.position, new Color(1, .45f, .1f));
            Game.sound.Play(SoundCue.Explosion);
            Retire();
        }

        private void Retire()
        {
            if (retired) return;
            retired = true;
            Game.Unregister(this);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

        private void OnDestroy() { if (GameController.Instance != null) Game.Unregister(this); }
    }
}
