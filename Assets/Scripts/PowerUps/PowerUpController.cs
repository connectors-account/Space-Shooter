using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Falling collectible that applies a temporary or instant effect.
    /// </summary>
    public class PowerUpController : MonoBehaviour
    {
        public enum PowerUpType
        {
            HealthRestore,
            WeaponUpgrade,
            Shield
        }

        [Header("Type")]
        [SerializeField] private PowerUpType powerUpType = PowerUpType.HealthRestore;

        [Header("Movement")]
        [SerializeField] private float driftSpeed = 2f;
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobFrequency = 3f;
        [SerializeField] private float lifetime = 12f;

        [Header("Effect Values")]
        [SerializeField] private int healthRestoreAmount = 30;

        [Header("Visual")]
        [SerializeField] private float pulseScaleAmount = 0.08f;

        private float spawnTime;
        private Vector3 initialScale;

        private void Start()
        {
            gameObject.tag = "PowerUp";
            spawnTime = Time.time;
            initialScale = transform.localScale;
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            float elapsed = Time.time - spawnTime;
            float bobOffset = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;

            Vector3 position = transform.position;
            position.y -= driftSpeed * Time.deltaTime;
            position.x += bobOffset * Time.deltaTime;
            transform.position = position;

            float pulse = 1f + Mathf.Sin(elapsed * bobFrequency * 1.6f) * pulseScaleAmount;
            transform.localScale = initialScale * pulse;

            if (transform.position.y < -7.5f)
            {
                Destroy(gameObject);
            }
        }

        public void ApplyEffect(Player.PlayerController player)
        {
            if (player == null)
            {
                return;
            }

            switch (powerUpType)
            {
                case PowerUpType.HealthRestore:
                    player.Heal(healthRestoreAmount);
                    break;
                case PowerUpType.WeaponUpgrade:
                    player.ActivateRapidFire();
                    break;
                case PowerUpType.Shield:
                    player.ActivateShield();
                    break;
            }

            Managers.AudioManager.Instance?.PlayPowerUpSound();
        }
    }
}
