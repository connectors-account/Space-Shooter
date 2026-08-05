using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Abstract base for all power-ups. Pooled objects that drift gently down,
    /// bob up and down, auto-recycle after a lifetime if not collected, and
    /// apply their effect when the player touches them. Concrete power-ups
    /// implement <see cref="Apply"/>.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public abstract class PowerUpBase : MonoBehaviour
    {
        [Header("Common")]
        [SerializeField] protected float duration = 6f;
        [SerializeField] protected float fallSpeed = 1.5f;
        [SerializeField] protected float lifeTime = 10f;
        [SerializeField] protected float bobAmplitude = 0.15f;
        [SerializeField] protected float bobFrequency = 3f;

        protected SpriteRenderer Renderer;
        protected Collider2D Collider;
        private float _life;
        private float _bobPhase;
        private bool _collected;

        /// <summary>The type of this power-up (also selects its sprite).</summary>
        public abstract PowerUpType Type { get; }

        /// <summary>The pool key used to recycle this power-up.</summary>
        protected abstract string PoolKey { get; }

        /// <summary>Apply the effect to the player. Implemented per power-up.</summary>
        public abstract void Apply(PlayerController controller, PlayerHealth health, PlayerShooter shooter);

        protected virtual void Awake()
        {
            Renderer = GetComponent<SpriteRenderer>();
            Collider = GetComponent<Collider2D>();
            Collider.isTrigger = true;
            Renderer.sortingOrder = 4;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            if (Renderer.sprite == null)
                Renderer.sprite = SpriteGenerator.CreatePowerUpSprite(Type);

            gameObject.tag = Constants.TagPowerUp;
        }

        protected virtual void OnEnable()
        {
            _life = lifeTime;
            _collected = false;
            _bobPhase = Random.Range(0f, Mathf.PI * 2f);
            if (Renderer != null && Renderer.sprite == null)
                Renderer.sprite = SpriteGenerator.CreatePowerUpSprite(Type);
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            float dt = Time.deltaTime;
            _bobPhase += bobFrequency * dt;
            float bob = Mathf.Sin(_bobPhase) * bobAmplitude;

            transform.position += new Vector3(0f, -fallSpeed * dt + bob * dt, 0f);

            _life -= dt;
            if (_life <= 0f)
                Recycle();

            // Recycle if it drifts off the bottom of the screen.
            var cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                float bottom = cam.transform.position.y - cam.orthographicSize - 1.5f;
                if (transform.position.y < bottom)
                    Recycle();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag(Constants.TagPlayer)) return;

            var controller = other.GetComponent<PlayerController>();
            var health = other.GetComponent<PlayerHealth>();
            var shooter = other.GetComponent<PlayerShooter>();
            Collect(controller, health, shooter);
        }

        /// <summary>Called when the player picks up the power-up.</summary>
        public void Collect(PlayerController controller, PlayerHealth health, PlayerShooter shooter)
        {
            if (_collected) return;
            _collected = true;

            Apply(controller, health, shooter);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(Constants.SfxPowerUp);

            // Notify listeners (HUD) that a power-up was collected.
            PowerUpEvents.RaiseCollected(Type, duration);

            Recycle();
        }

        protected void Recycle()
        {
            if (ObjectPool.Instance != null)
                ObjectPool.Instance.Release(PoolKey, gameObject);
            else
                gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Static broadcaster so the HUD can react to power-up collection without a
    /// hard reference to each power-up instance.
    /// </summary>
    public static class PowerUpEvents
    {
        public static event System.Action<PowerUpType, float> OnCollected;
        public static void RaiseCollected(PowerUpType type, float duration) => OnCollected?.Invoke(type, duration);
    }
}
