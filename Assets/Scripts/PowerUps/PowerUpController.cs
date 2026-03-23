using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Power-up types: HealthPack, RapidFire, Shield.
    /// Drifts downward and applies effect on player contact.
    /// </summary>
    public class PowerUpController : MonoBehaviour
    {
        public enum PowerUpType
        {
            HealthPack,
            RapidFire,
            Shield
        }

        [Header("Power-Up Settings")]
        [SerializeField] private PowerUpType powerUpType = PowerUpType.HealthPack;
        [SerializeField] private float driftSpeed = 2f;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private int healAmount = 30;

        [Header("Visual")]
        [SerializeField] private float bobAmplitude = 0.3f;
        [SerializeField] private float bobFrequency = 2f;

        private float spawnTime;
        private float startY;

        private void Start()
        {
            spawnTime = Time.time;
            startY = transform.position.y;
            tag = "PowerUp";

            // Auto-destroy after lifetime
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // Drift downward with a gentle bobbing motion
            float elapsed = Time.time - spawnTime;
            float bobOffset = Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;

            transform.position += Vector3.down * driftSpeed * Time.deltaTime;

            // Apply bob on local scale (visual wobble)
            float scale = 1f + Mathf.Sin(elapsed * bobFrequency * 2f) * 0.05f;
            transform.localScale = new Vector3(scale, scale, 1f);

            // Destroy if out of bounds
            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Applies this power-up's effect to the player.
        /// Called by PlayerController on trigger collision.
        /// </summary>
        public void ApplyEffect(Player.PlayerController player)
        {
            if (player == null) return;

            switch (powerUpType)
            {
                case PowerUpType.HealthPack:
                    player.Heal(healAmount);
                    break;

                case PowerUpType.RapidFire:
                    player.ActivateRapidFire();
                    break;

                case PowerUpType.Shield:
                    player.ActivateShield();
                    break;
            }

            // Play power-up sound
            Managers.AudioManager.Instance?.PlayPowerUpSound();
        }
    }
}
