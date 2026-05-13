// ============================================================================
// PowerUp.cs — Falling pick-up items the player can collect
// Each instance is configured by PowerUpType. Drifts downward and self-destructs
// when off-screen.
// ============================================================================
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;

namespace SpaceShooter.PowerUps
{
    public enum PowerUpType
    {
        Health,
        Shield,
        RapidFire,
        SpreadShot
    }

    public class PowerUp : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private PowerUpType type = PowerUpType.Health;
        [SerializeField] private float fallSpeed = 2f;
        [SerializeField] private int healAmount = 30;

        [Header("Visual")]
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobFrequency = 3f;

        private float _startY;
        private float _time;

        private void OnEnable()
        {
            _startY = transform.position.y;
            _time = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
                return;

            _time += Time.deltaTime;

            // Move downward with a gentle bob
            float y = transform.position.y - fallSpeed * Time.deltaTime;
            float bob = Mathf.Sin(_time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
            transform.position = new Vector3(transform.position.x + bob * Time.deltaTime, y, 0f);

            // Off-screen cleanup
            if (GameBounds.Instance != null && GameBounds.Instance.IsOutOfBounds(transform.position))
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            ApplyEffect(other.gameObject);
            AudioManager.Instance?.PlaySFX("PowerUp");
            Destroy(gameObject);
        }

        private void ApplyEffect(GameObject player)
        {
            switch (type)
            {
                case PowerUpType.Health:
                    player.GetComponent<Player.PlayerHealth>()?.Heal(healAmount);
                    break;

                case PowerUpType.Shield:
                    player.GetComponent<Player.PlayerHealth>()?.ActivateShield();
                    break;

                case PowerUpType.RapidFire:
                    player.GetComponent<Player.PlayerShooting>()?.ActivateRapidFire();
                    break;

                case PowerUpType.SpreadShot:
                    player.GetComponent<Player.PlayerShooting>()?.ActivateSpreadShot();
                    break;
            }
        }
    }
}
