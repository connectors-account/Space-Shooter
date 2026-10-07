using UnityEngine;

namespace StellarShooter
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Enemy : MonoBehaviour
    {
        [Range(0, 2)] public int kind;
        public Projectile projectilePrefab;
        private GameSession game;
        private Rigidbody2D body;
        private int health, wave;
        private float nextShot, age, originX;
        private bool dead;
        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        public void Initialize(GameSession session, int waveNumber)
        {
            game = session;
            wave = waveNumber;
            health = GameRules.EnemyHealth(wave, kind);
            originX = transform.position.x;
            nextShot = Time.time + 1.2f;
        }
        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying || dead) return;
            age += Time.fixedDeltaTime;
            float sway = kind == 0 ? Mathf.Sin(age * 2) * .85f : Mathf.Sin(age) * .35f;
            body.MovePosition(new Vector2(originX + sway, body.position.y - GameRules.EnemySpeed(wave) * Time.fixedDeltaTime));
        }
        private void Update()
        {
            if (game == null || !game.IsPlaying || dead) return;
            if (transform.position.y < -5.8f)
            {
                dead = true;
                game.Player.TakeDamage(1); // Escaping enemies cost hull unless protected.
                Destroy(gameObject);
                return;
            }
            if (Time.time >= nextShot && transform.position.y < 4.4f && transform.position.y > -3.5f)
            {
                nextShot = Time.time + GameRules.FireInterval(wave);
                FirePattern();
            }
        }
        private void FirePattern()
        {
            Vector2 aim = Vector2.down;
            if (kind == 0 && game.Player != null)
                aim = ((Vector2)(game.Player.transform.position - transform.position)).normalized;
            for (int i = 0; i < GameRules.PatternCount(kind); i++)
            {
                Vector2 direction = Quaternion.Euler(0, 0, GameRules.PatternAngle(kind, i)) * aim;
                Projectile shot = Instantiate(projectilePrefab, transform.position + Vector3.down * .4f, Quaternion.identity, game.actors);
                shot.Launch(game, direction, kind == 2 ? 2.7f : 3.8f, false);
            }
            game.sound.Play(SoundCue.EnemyFire);
        }
        public void TakeDamage(int damage)
        {
            if (dead || !game.IsPlaying) return;
            health -= damage;
            if (health > 0)
            {
                game.Impact(transform.position, Color.white, .4f);
                return;
            }
            Die(true);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || dead || !game.IsPlaying) return;
            player.TakeDamage(1);
            Die(false);
        }
        private void Die(bool awardScore)
        {
            dead = true;
            GetComponent<Collider2D>().enabled = false;
            if (awardScore) game.AwardKill(kind, transform.position);
            game.Impact(transform.position, new Color(1, .48f, .2f), 1.5f);
            game.sound.Play(SoundCue.Explosion);
            Destroy(gameObject);
        }
    }
}
