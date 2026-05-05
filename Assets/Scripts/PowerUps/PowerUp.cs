using SpaceShooter.Core;
using SpaceShooter.Player;
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    public enum PowerUpType
    {
        WeaponUpgrade,
        Health,
        Shield
    }

    public class PowerUp : MonoBehaviour
    {
        private ObjectPoolManager _pool;
        private GameManager _gameManager;
        private GameConfig _config;

        public PowerUpType Type { get; private set; }

        public void Initialize(ObjectPoolManager pool, GameManager gameManager, GameConfig config, PowerUpType type)
        {
            _pool = pool;
            _gameManager = gameManager;
            _config = config;
            Type = type;
        }

        private void Update()
        {
            if (_gameManager.CurrentState != GameState.Playing) return;

            transform.position += Vector3.down * (_config.PowerUpFallSpeed * Time.deltaTime);
            if (transform.position.y < -6.5f)
            {
                _pool.Release(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController player)) return;
            var health = other.GetComponent<PlayerHealth>();

            switch (Type)
            {
                case PowerUpType.WeaponUpgrade:
                    player.UpgradeWeapon();
                    break;
                case PowerUpType.Health:
                    health.Heal(_config.HealthPowerUpAmount);
                    break;
                case PowerUpType.Shield:
                    health.ActivateShield(_config.ShieldDuration);
                    break;
            }

            Sound.SoundManager.Instance?.PlaySfx("powerup");
            _pool.Release(gameObject);
        }
    }
}
