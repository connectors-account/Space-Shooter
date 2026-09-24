using UnityEngine;

namespace Starward
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Health))]
    public sealed class Enemy : MonoBehaviour
    {
        GameManager game;
        Health health;
        Rigidbody2D body;
        float speed, nextShot, interval, drift;
        bool fan, removed;
        void Awake()
        {
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
            health.Died += Defeated;
        }
        public void Initialize(GameManager owner, int wave, bool fanPattern)
        {
            game = owner;
            fan = fanPattern;
            speed = GameRules.EnemySpeed(wave);
            interval = GameRules.FireDelay(wave);
            drift = fan ? .65f : -.3f;
            nextShot = Time.time + 1.1f;
            health.ResetHealth(GameRules.EnemyHealth(wave));
            GetComponent<SpriteRenderer>().color = fan ? new Color(1, .65f, 1) : Color.white;
        }
        void Update()
        {
            if (game == null || !game.IsPlaying || removed) return;
            if (transform.position.y < -6.3f)
            {
                Remove(false);
                if (game.Player != null) game.Player.Hit(1);
                return;
            }
            if (Time.time >= nextShot && transform.position.y < 5f && transform.position.y > -4f)
            {
                nextShot = Time.time + interval;
                int count = fan ? 3 : 1;
                for (int i = 0; i < count; i++)
                {
                    float angle = GameRules.ShotAngle(i, count, 18) * Mathf.Deg2Rad;
                    game.Fire(false, transform.position + Vector3.down * .55f, new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)));
                }
                game.audioBank.Play(SoundId.EnemyShot);
            }
        }
        void FixedUpdate()
        {
            if (game == null || !game.IsPlaying || removed) return;
            Vector2 p = body.position;
            if (p.x < -9 || p.x > 9) drift = -drift;
            body.MovePosition(p + new Vector2(drift, -speed) * Time.fixedDeltaTime);
        }
        public void Hit(int damage) { if (!removed && game.IsPlaying) health.Damage(damage); }
        void Defeated() { Remove(true); }
        void Remove(bool defeated)
        {
            if (removed) return;
            removed = true;
            game.EnemyRemoved(transform.position, defeated);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (removed || game == null || !game.IsPlaying) return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null) { player.Hit(1); Remove(false); }
        }
        void OnDestroy() { if (health != null) health.Died -= Defeated; }
    }
}
