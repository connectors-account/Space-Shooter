using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class PlayerShip : MonoBehaviour
    {
        public float movementSpeed = 10f;
        public const int MaxHealth = 5;
        public int Health { get; private set; } = MaxHealth;
        public int ShieldHits { get; private set; }
        public float RapidSeconds => Mathf.Max(0, rapidUntil - Time.time);
        public float ShieldSeconds => Mathf.Max(0, shieldUntil - Time.time);
        public bool MouseControl { get; private set; }
        private Rigidbody2D body;
        private SpriteRenderer visual;
        private Vector2 movement;
        private Vector2 mouseTarget;
        private float nextShot;
        private float invulnerableUntil;
        private float rapidUntil;
        private float shieldUntil;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            visual = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (!GameController.Instance.IsPlaying) return;
            if (Input.GetKeyDown(KeyCode.M)) MouseControl = !MouseControl;
            float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0)
                    - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float y = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0)
                    - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
            movement = Vector2.ClampMagnitude(new Vector2(x, y), 1);
            mouseTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0)) && Time.time >= nextShot)
            {
                bool rapid = RapidSeconds > 0;
                nextShot = Time.time + (rapid ? 0.10f : 0.22f);
                Vector2 origin = body.position + Vector2.up * 0.8f;
                GameController.Instance.Fire(true, origin, Vector2.up, 22f);
                if (rapid)
                {
                    GameController.Instance.Fire(true, origin + Vector2.left * 0.22f, new Vector2(-0.16f, 1), 22f);
                    GameController.Instance.Fire(true, origin + Vector2.right * 0.22f, new Vector2(0.16f, 1), 22f);
                }
                GameController.Instance.audioSystem.Play(SoundCue.Laser);
            }
            if (ShieldSeconds <= 0) ShieldHits = 0;
            bool flash = Time.time < invulnerableUntil && Mathf.FloorToInt(Time.time * 16f) % 2 == 0;
            visual.color = flash ? new Color(1, 1, 1, 0.3f) : ShieldHits > 0 ? new Color(0.55f, 0.8f, 1) : Color.white;
        }

        private void FixedUpdate()
        {
            if (!GameController.Instance.IsPlaying) return;
            Vector2 target = MouseControl ? Vector2.MoveTowards(body.position, mouseTarget, movementSpeed * Time.fixedDeltaTime)
                : body.position + movement * movementSpeed * Time.fixedDeltaTime;
            float edge = GameController.Instance.HalfWidth - 0.75f;
            target.x = Mathf.Clamp(target.x, -edge, edge);
            target.y = Mathf.Clamp(target.y, -7.9f, 7.2f);
            body.MovePosition(target);
        }

        public void Damage(int amount)
        {
            if (!GameController.Instance.IsPlaying || Time.time < invulnerableUntil || Health <= 0) return;
            invulnerableUntil = Time.time + 1.1f;
            if (ShieldHits > 0 && ShieldSeconds > 0) ShieldHits--;
            else Health = Mathf.Max(0, Health - amount);
            GameController.Instance.audioSystem.Play(SoundCue.Hit);
            if (Health == 0) GameController.Instance.EndGame();
        }

        public void Collect(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Repair:
                    if (Health == MaxHealth) GameController.Instance.Announce("HULL ALREADY FULL");
                    else { Health = Mathf.Min(MaxHealth, Health + 2); GameController.Instance.Announce("HULL REPAIRED +2"); }
                    break;
                case PickupKind.RapidFire:
                    rapidUntil = Time.time + 12f;
                    GameController.Instance.Announce("TRIPLE SHOT / 12 SECONDS");
                    break;
                case PickupKind.Shield:
                    ShieldHits = 3;
                    shieldUntil = Time.time + 15f;
                    GameController.Instance.Announce("SHIELD / 3 HITS / 15 SECONDS");
                    break;
            }
            GameController.Instance.audioSystem.Play(SoundCue.Pickup);
        }
    }
}
