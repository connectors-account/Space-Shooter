using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// A collectable power-up that drifts down the screen. When the player touches it the configured
    /// <see cref="PowerUpType"/> effect is applied. Power-ups are pooled.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class PowerUp : MonoBehaviour, IPoolable
    {
        private SpriteRenderer _renderer;
        private CircleCollider2D _collider;
        private GameConfig _config;
        private PowerUpType _type;
        private float _fallSpeed;
        private float _bobTimer;

        /// <summary>The effect this pickup grants.</summary>
        public PowerUpType Type => _type;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
            _collider.radius = 0.32f;
            _renderer.sortingOrder = 4;
        }

        /// <summary>
        /// Configures the power-up before it is released into the world.
        /// </summary>
        /// <param name="config">Shared configuration.</param>
        /// <param name="type">Effect to grant on pickup.</param>
        public void Configure(GameConfig config, PowerUpType type)
        {
            _config = config;
            _type = type;
            _fallSpeed = config.PowerUpFallSpeed;
            _bobTimer = 0f;

            _renderer.sprite = SpriteFactory.CreateCircleSprite(Color.white, 32);
            _renderer.color = ColorFor(type);
            transform.localScale = Vector3.one * 0.8f;
        }

        /// <summary>Returns the representative colour for a power-up type.</summary>
        public static Color ColorFor(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Health: return new Color(0.2f, 1f, 0.3f);
                case PowerUpType.Shield: return new Color(0.3f, 0.7f, 1f);
                case PowerUpType.RapidFire: return new Color(1f, 0.85f, 0.2f);
                case PowerUpType.SpreadShot: return new Color(1f, 0.5f, 0.1f);
                case PowerUpType.ScoreMultiplier: return new Color(0.9f, 0.3f, 1f);
                default: return Color.white;
            }
        }

        private void Update()
        {
            _bobTimer += Time.deltaTime * 6f;
            float wobble = Mathf.Sin(_bobTimer) * 0.6f;
            transform.position += new Vector3(wobble * Time.deltaTime, -_fallSpeed * Time.deltaTime, 0f);

            if (transform.position.y < -_config.HalfHeight - 1f)
            {
                PowerUpManager.Instance?.Release(gameObject);
            }
        }

        /// <summary>
        /// Called by the collision system when the player collects this power-up.
        /// </summary>
        public void Collect()
        {
            PowerUpManager.Instance?.Release(gameObject);
        }

        /// <inheritdoc />
        public void OnSpawned()
        {
            _bobTimer = 0f;
        }

        /// <inheritdoc />
        public void OnDespawned()
        {
        }
    }
}
