using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Player;

namespace SpaceShooter.Pickups
{
    /// <summary>Types of collectible power-ups. Also used to key sprite colours.</summary>
    public enum PowerUpType { Shield, TripleShot, Spread5, SpeedBoost, Laser, HealthPack, Nuke }

    /// <summary>
    /// A drifting, rotating collectible. On contact with the player it applies its
    /// effect and returns to the pool. Nuke clears the screen of enemies.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class PowerUp : MonoBehaviour
    {
        #region Fields
        [SerializeField] private PowerUpType _type = PowerUpType.Shield;
        private SpriteRenderer _renderer;
        #endregion

        #region Properties
        public PowerUpType Type => _type;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            gameObject.layer = GameConstants.LAYER_ID_POWERUP;
        }

        private void OnEnable()
        {
            ApplyColour();
        }

        private void Update()
        {
            transform.Translate(Vector3.down * GameConstants.POWERUP_DRIFT_SPEED * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.forward, GameConstants.POWERUP_ROTATE_SPEED * Time.deltaTime, Space.Self);

            if (transform.position.y < GameConstants.CAMERA_BOTTOM - 1.5f)
                ReturnToPool();
        }
        #endregion

        #region Configuration
        /// <summary>Sets the power-up type (used when spawning from the pool).</summary>
        public void Configure(PowerUpType type)
        {
            _type = type;
            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            ApplyColour();
        }

        private void ApplyColour()
        {
            if (_renderer == null) return;
            _renderer.color = GetColour(_type);
        }

        /// <summary>Returns the distinct display colour for a power-up type.</summary>
        public static Color GetColour(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield: return Color.cyan;
                case PowerUpType.TripleShot: return Color.green;
                case PowerUpType.Spread5: return Color.yellow;
                case PowerUpType.SpeedBoost: return new Color(1f, 0.55f, 0f); // orange
                case PowerUpType.Laser: return Color.magenta;
                case PowerUpType.HealthPack: return Color.red;
                case PowerUpType.Nuke: return Color.white;
                default: return Color.white;
            }
        }
        #endregion

        #region Collision
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(GameConstants.TAG_PLAYER)) return;

            if (_type == PowerUpType.Nuke)
            {
                if (WaveManager.Instance != null) WaveManager.Instance.NukeAllEnemies();
            }
            else
            {
                PlayerPowerUp pp = other.GetComponent<PlayerPowerUp>();
                if (pp != null) pp.ApplyPowerUp(_type);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.PowerUp);

            ReturnToPool();
        }
        #endregion

        #region Pool
        private void ReturnToPool()
        {
            if (PowerUpSpawner.Pool != null)
                PowerUpSpawner.Pool.Return(gameObject);
            else
                gameObject.SetActive(false);
        }
        #endregion
    }
}
