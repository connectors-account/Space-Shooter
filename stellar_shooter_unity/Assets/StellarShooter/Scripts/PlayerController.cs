using UnityEngine;

namespace StellarShooter
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class PlayerController : MonoBehaviour
    {
        public Projectile projectilePrefab;
        public SpriteRenderer shieldVisual;
        public int Health { get; private set; } = GameRules.MaxHealth;
        public float SpreadRemaining => Mathf.Max(0, spreadUntil - Time.time);
        public float ShieldRemaining => Mathf.Max(0, shieldUntil - Time.time);
        private GameSession game;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private Vector2 movement;
        private float nextShot, invulnerableUntil, spreadUntil, shieldUntil;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
        }
        public void Initialize(GameSession session) { game = session; }
        private void Update()
        {
            if (game == null || !game.IsPlaying) return;
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
            movement = Vector2.ClampMagnitude(new Vector2(x, y), 1);
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextShot)
            {
                nextShot = Time.time + .18f;
                Fire(0);
                if (SpreadRemaining > 0) { Fire(-14); Fire(14); }
                game.sound.Play(SoundCue.Shoot);
            }
            shieldVisual.enabled = ShieldRemaining > 0;
            sprite.color = Time.time < invulnerableUntil && Mathf.Sin(Time.time * 40) > 0
                ? new Color(1, 1, 1, .35f) : Color.white;
        }
        private static bool Held(KeyCode first, KeyCode second) => Input.GetKey(first) || Input.GetKey(second);
        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying) return;
            Vector2 next = body.position + movement * (GameRules.PlayerSpeed * Time.fixedDeltaTime);
            next.x = Mathf.Clamp(next.x, -GameRules.HalfWidth + .6f, GameRules.HalfWidth - .6f);
            next.y = Mathf.Clamp(next.y, -4.35f, 3.55f);
            body.MovePosition(next);
        }
        private void Fire(float angle)
        {
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            Projectile shot = Instantiate(projectilePrefab, transform.position + Vector3.up * .62f, Quaternion.identity, game.actors);
            shot.Launch(game, direction, 13, true);
        }
        public void StopMovement() { movement = Vector2.zero; body.linearVelocity = Vector2.zero; }
        public void TakeDamage(int amount)
        {
            if (game == null || !game.IsPlaying || Time.time < invulnerableUntil || ShieldRemaining > 0) return;
            Health = Mathf.Max(0, Health - amount);
            invulnerableUntil = Time.time + 1.15f;
            game.sound.Play(SoundCue.Hit);
            game.Impact(transform.position, new Color(.4f, .85f, 1), .75f);
            if (Health == 0)
            {
                StopMovement();
                sprite.enabled = false;
                shieldVisual.enabled = false;
                game.EndGame();
            }
        }
        public void ApplyPickup(PickupKind kind)
        {
            if (kind == PickupKind.Repair) Health = GameRules.Heal(Health);
            if (kind == PickupKind.Spread) spreadUntil = Time.time + GameRules.SpreadDuration;
            if (kind == PickupKind.Shield) shieldUntil = Time.time + GameRules.ShieldDuration;
            game.sound.Play(SoundCue.Pickup);
        }
    }
}
