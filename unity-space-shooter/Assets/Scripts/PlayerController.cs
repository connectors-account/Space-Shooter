using UnityEngine;

namespace Starward
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Health), typeof(SpriteRenderer))]
    public sealed class PlayerController : MonoBehaviour
    {
        public float speed = 7.5f;
        public Health Health { get; private set; }
        public int WeaponLevel { get; private set; }
        GameManager game;
        Rigidbody2D body;
        SpriteRenderer sprite;
        Vector2 input;
        float nextShot;
        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            Health = GetComponent<Health>();
            sprite = GetComponent<SpriteRenderer>();
            Health.Died += Die;
        }
        public void Initialize(GameManager owner)
        {
            game = owner;
            WeaponLevel = 1;
            Health.ResetHealth(GameRules.MaxHealth, 2);
        }
        void Update()
        {
            if (game == null || !game.IsPlaying) { input = Vector2.zero; return; }
            // Key codes avoid dependence on an InputManager axis configuration.
            float x = (Held(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0);
            float y = (Held(KeyCode.W, KeyCode.UpArrow) ? 1 : 0) - (Held(KeyCode.S, KeyCode.DownArrow) ? 1 : 0);
            input = Vector2.ClampMagnitude(new Vector2(x, y), 1);
            sprite.color = Health.Invulnerable && Mathf.Sin(Time.time * 35) > 0 ? new Color(.4f, 1, 1, .4f) : Color.white;
            if (Input.GetKey(KeyCode.Space) && Time.time >= nextShot)
            {
                nextShot = Time.time + .18f;
                int count = GameRules.ShotCount(WeaponLevel);
                for (int i = 0; i < count; i++)
                {
                    float angle = GameRules.ShotAngle(i, count, 9) * Mathf.Deg2Rad;
                    game.Fire(true, transform.position + Vector3.up * .65f, new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)));
                }
                game.audioBank.Play(SoundId.Laser);
            }
        }
        static bool Held(KeyCode a, KeyCode b) { return Input.GetKey(a) || Input.GetKey(b); }
        void FixedUpdate()
        {
            if (game == null || !game.IsPlaying) return;
            Vector2 next = body.position + input * speed * Time.fixedDeltaTime;
            next.x = Mathf.Clamp(next.x, GameRules.Left, GameRules.Right);
            next.y = Mathf.Clamp(next.y, GameRules.Bottom, GameRules.Top);
            body.MovePosition(next);
        }
        public void Hit(int damage)
        {
            if (game.IsPlaying && Health.Damage(damage, 1.25f))
            {
                WeaponLevel = Mathf.Max(1, WeaponLevel - 1);
                game.audioBank.Play(SoundId.Hit);
                if (Health.Current > 0) game.SpawnBurst(transform.position, Color.cyan);
            }
        }
        public void Upgrade() { WeaponLevel = Mathf.Min(GameRules.MaxWeapon, WeaponLevel + 1); }
        void Die()
        {
            sprite.enabled = false;
            game.EndRun();
        }
        void OnDestroy() { if (Health != null) Health.Died -= Die; }
    }
}
