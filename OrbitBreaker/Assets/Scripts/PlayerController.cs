using UnityEngine;

namespace OrbitBreaker
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class PlayerController : MonoBehaviour
    {
        public const int MaxHealth = 5;
        public int Health { get; private set; } = MaxHealth;
        public float RapidRemaining => Mathf.Max(0f, rapidUntil - Time.time);
        public float TripleRemaining => Mathf.Max(0f, tripleUntil - Time.time);
        private GameController game;
        private Rigidbody2D body;
        private SpriteRenderer sprite;
        private Vector2 input;
        private bool firing;
        private float nextShot, invincibleUntil, rapidUntil, tripleUntil;
        private const float Speed = 6f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            sprite = GetComponent<SpriteRenderer>();
        }

        public void Initialize(GameController controller)
        {
            game = controller;
            Health = MaxHealth;
            invincibleUntil = Time.time + 1.2f;
        }

        private void Update()
        {
            if (game == null || game.Phase != GamePhase.Playing) { ClearInput(); return; }
            // KeyCode polling needs no named axes or Input Actions asset.
            input = new Vector2((Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0),
                (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0));
            input = Vector2.ClampMagnitude(input, 1f);
            firing = Input.GetKey(KeyCode.Space);
            sprite.color = Time.time < invincibleUntil && Mathf.FloorToInt(Time.time * 14f) % 2 == 0
                ? new Color(1f, 1f, 1f, .32f) : Color.white;
            if (firing && Time.time >= nextShot)
            {
                nextShot = Time.time + (Time.time < rapidUntil ? .09f : .19f);
                Vector2 muzzle = body.position + Vector2.up * .62f;
                game.Fire(muzzle, Vector2.up, true, 12f);
                if (Time.time < tripleUntil)
                {
                    game.Fire(muzzle, new Vector2(-.23f, 1f), true, 12f);
                    game.Fire(muzzle, new Vector2(.23f, 1f), true, 12f);
                }
                game.Play(game.assets.shoot, .22f);
            }
        }

        private static bool Held(KeyCode a, KeyCode b) { return Input.GetKey(a) || Input.GetKey(b); }

        private void FixedUpdate()
        {
            if (game == null || game.Phase != GamePhase.Playing) { body.velocity = Vector2.zero; return; }
            Vector2 target = body.position + input * (Speed * Time.fixedDeltaTime);
            target.x = Mathf.Clamp(target.x, -GameController.HalfWidth, GameController.HalfWidth);
            target.y = Mathf.Clamp(target.y, GameController.Bottom, GameController.Top);
            body.MovePosition(target);
        }

        public void ClearInput() { input = Vector2.zero; firing = false; if (body != null) body.velocity = Vector2.zero; }

        public void Hit()
        {
            if (game == null || game.Phase != GamePhase.Playing || Time.time < invincibleUntil) return;
            Health = Mathf.Max(0, Health - 1);
            invincibleUntil = Time.time + 1.2f;
            game.Play(game.assets.hit);
            game.Burst(transform.position, new Color(.3f, .9f, 1f));
            if (Health == 0) game.EndGame();
        }

        public void Collect(PowerupKind kind)
        {
            switch (kind)
            {
                case PowerupKind.Repair: Health = Mathf.Min(MaxHealth, Health + 1); break;
                case PowerupKind.Rapid: rapidUntil = Time.time + 10f; break;
                case PowerupKind.Triple: tripleUntil = Time.time + 12f; break;
            }
            game.Play(game.assets.pickup);
            game.Burst(transform.position, new Color(.4f, 1f, .6f));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Bullet and pickup scripts own those interactions; only ship contact is handled here.
            if (other.TryGetComponent(out Enemy enemy) && enemy.IsAlive) Hit();
        }
    }
}
