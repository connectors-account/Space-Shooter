using UnityEngine;

namespace NovaGuard
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public sealed class PlayerShip : MonoBehaviour
    {
        public int maxHealth = 5;
        public float moveSpeed = 8f;
        public float fireInterval = 0.22f;
        public float hitInvulnerability = 1.25f;
        public int Health { get; private set; }
        public float RapidRemaining => Mathf.Max(0, rapidUntil - Time.time);
        public float ShieldRemaining => Mathf.Max(0, shieldUntil - Time.time);
        private Rigidbody2D body;
        private SpriteRenderer image;
        private float nextFire, invulnerableUntil, rapidUntil, shieldUntil;
        private bool dead;
        private GameController Game => GameController.Instance;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            image = GetComponent<SpriteRenderer>();
            Health = maxHealth;
            invulnerableUntil = Time.time + 1.5f;
        }

        private void FixedUpdate()
        {
            if (!Game.IsPlaying || dead) { body.velocity = Vector2.zero; return; }
            Vector2 next = body.position + KeyboardInput.Movement * (moveSpeed * Time.fixedDeltaTime);
            next.x = Mathf.Clamp(next.x, GameController.Left, GameController.Right);
            next.y = Mathf.Clamp(next.y, GameController.Bottom, GameController.Top);
            body.MovePosition(next);
        }

        private void Update()
        {
            if (!Game.IsPlaying || dead) return;
            bool blink = Time.time < invulnerableUntil && Mathf.FloorToInt(Time.time * 14) % 2 == 0;
            image.color = ShieldRemaining > 0 ? new Color(0.45f, 0.85f, 1) : Color.white;
            if (blink) image.color = new Color(image.color.r, image.color.g, image.color.b, 0.35f);
            if (!KeyboardInput.FireHeld || Time.time < nextFire) return;
            bool rapid = RapidRemaining > 0;
            nextFire = Time.time + (rapid ? 0.12f : fireInterval);
            Vector2 muzzle = (Vector2)transform.position + Vector2.up * 0.75f;
            Game.Fire(muzzle, Vector2.up, true, 20f);
            if (rapid)
            {
                Game.Fire(muzzle + Vector2.left * 0.22f, new Vector2(-0.13f, 1), true, 20f);
                Game.Fire(muzzle + Vector2.right * 0.22f, new Vector2(0.13f, 1), true, 20f);
            }
            Game.sound.Play(SoundCue.Fire);
        }

        public void TakeDamage(int amount)
        {
            if (!Game.IsPlaying || dead || Time.time < invulnerableUntil || ShieldRemaining > 0) return;
            Health = Mathf.Max(0, Health - amount);
            invulnerableUntil = Time.time + hitInvulnerability;
            Game.sound.Play(SoundCue.Hit);
            Game.SpawnEffect(transform.position, new Color(0.3f, 0.85f, 1), 12);
            if (Health > 0) return;
            dead = true;
            image.enabled = false;
            GetComponent<Collider2D>().enabled = false;
            body.velocity = Vector2.zero;
            Game.EndRun();
        }

        public void Apply(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Repair:
                    Health = Mathf.Min(maxHealth, Health + 2);
                    Game.ui.Announce("REPAIR +2", 1.4f);
                    break;
                case PickupKind.Rapid:
                    rapidUntil = Time.time + 10;
                    Game.ui.Announce("TRIPLE SHOT / 10 SECONDS", 1.4f);
                    break;
                case PickupKind.Shield:
                    shieldUntil = Time.time + 8;
                    Game.ui.Announce("SHIELD / 8 SECONDS", 1.4f);
                    break;
            }
            Game.AddScore(25);
            Game.sound.Play(SoundCue.Pickup);
            Game.SpawnEffect(transform.position, Color.cyan, 10);
        }
    }
}
