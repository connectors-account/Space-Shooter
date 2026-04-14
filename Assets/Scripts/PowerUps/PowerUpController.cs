using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Power-up pickup behavior.
    /// Types: HealthPack, RapidFire, Shield.
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
        [SerializeField] private float bobFrequency = 2f;

        private float spawnTime;

        private void Start()
        {
            spawnTime = Time.time;
            tag = "PowerUp";
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            float elapsed = Time.time - spawnTime;

            transform.position += Vector3.down * driftSpeed * Time.deltaTime;

            float scale = 1f + Mathf.Sin(elapsed * bobFrequency * 2f) * 0.05f;
            transform.localScale = new Vector3(scale, scale, 1f);

            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }

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

            Managers.AudioManager.Instance?.PlayPowerUpSound();
        }
    }
}
