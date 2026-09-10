using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class PlayerShip : MonoBehaviour
    {
        public int Health { get; private set; } = GameRules.MaxHealth;
        public float RapidSeconds => Mathf.Max(0, rapidUntil - Time.time);
        public float SpreadSeconds => Mathf.Max(0, spreadUntil - Time.time);
        Rigidbody2D body;
        SpriteRenderer image;
        Vector2 movement;
        float nextShot, invulnerableUntil, rapidUntil, spreadUntil;
        bool dead;

        void Awake() { body = GetComponent<Rigidbody2D>(); image = GetComponent<SpriteRenderer>(); invulnerableUntil = Time.time + 1.5f; }
        void Update()
        {
            var game = StarGame.Instance;
            if (game.State != RunState.Playing || dead) { movement = Vector2.zero; return; }
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
            movement = Vector2.ClampMagnitude(new Vector2(x, y), 1) * (Input.GetKey(KeyCode.LeftShift) ? 3.4f : 6.5f);
            image.color = Time.time < invulnerableUntil && Mathf.Sin(Time.time * 38) > 0 ? new Color(1, 1, 1, 0.3f) : Color.white;
            if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z)) && Time.time >= nextShot)
            {
                nextShot = Time.time + GameRules.FireInterval(RapidSeconds > 0);
                Fire(0);
                if (SpreadSeconds > 0) { Fire(-14); Fire(14); }
                game.Sound(game.assets.shoot, 0.17f);
            }
        }
        static bool Held(KeyCode first, KeyCode second) { return Input.GetKey(first) || Input.GetKey(second); }
        void FixedUpdate()
        {
            if (StarGame.Instance.State != RunState.Playing || dead) return;
            Vector2 target = body.position + movement * Time.fixedDeltaTime;
            target.x = Mathf.Clamp(target.x, -8.25f, 8.25f);
            target.y = Mathf.Clamp(target.y, -4.35f, 3.9f);
            body.MovePosition(target);
        }
        void Fire(float angle)
        {
            Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
            Projectile.Spawn(StarGame.Instance.assets.playerBullet, transform.position + Vector3.up * 0.58f, direction, 14);
        }
        public void Damage()
        {
            var game = StarGame.Instance;
            if (dead || game.State != RunState.Playing || Time.time < invulnerableUntil) return;
            Health--; invulnerableUntil = Time.time + 1.25f;
            game.Sound(game.assets.hurt, 0.6f);
            game.Burst(transform.position, new Color(0.3f, 0.9f, 1), 12);
            if (Health <= 0) { dead = true; image.enabled = false; game.EndRun(); }
        }
        public void Collect(PowerKind kind)
        {
            if (dead) return;
            if (kind == PowerKind.Repair) Health = GameRules.Heal(Health);
            if (kind == PowerKind.Rapid) rapidUntil = Time.time + 9;
            if (kind == PowerKind.Spread) spreadUntil = Time.time + 12;
            StarGame.Instance.Sound(StarGame.Instance.assets.collect, 0.5f);
            StarGame.Instance.Burst(transform.position, new Color(0.4f, 1, 0.65f), 10);
        }
    }
}
