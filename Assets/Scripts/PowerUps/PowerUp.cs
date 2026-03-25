// =============================================================================
// PowerUp.cs — Collectible power-up items
// =============================================================================
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Types of power-ups available in the game.
    /// </summary>
    public enum PowerUpType
    {
        Health,
        Shield,
        RapidFire,
        SpreadShot,
        ExtraLife,
        ScoreBonus
    }

    /// <summary>
    /// Power-up collectible that drifts downward and applies effects on pickup.
    /// </summary>
    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpType type = PowerUpType.Health;
        [SerializeField] private float fallSpeed = 2f;
        [SerializeField] private float lifetime = 10f;
        [SerializeField] private int healAmount = 2;
        [SerializeField] private int scoreBonus = 500;

        [Header("Visual")]
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobFrequency = 3f;
        [SerializeField] private float rotateSpeed = 90f;

        /// <summary>The type of this power-up.</summary>
        public PowerUpType Type => type;

        private float spawnTime;
        private float startY;

        private void Start()
        {
            spawnTime = Time.time;
            startY = transform.position.y;
        }

        private void Update()
        {
            // Drift downward with bobbing
            float elapsed = Time.time - spawnTime;
            float y = startY - fallSpeed * elapsed + Mathf.Sin(elapsed * bobFrequency) * bobAmplitude;
            transform.position = new Vector3(transform.position.x, y, 0f);

            // Gentle rotation
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

            // Self-destruct after lifetime
            if (elapsed > lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            Player.PlayerController player = other.GetComponent<Player.PlayerController>();
            if (player == null) return;

            ApplyEffect(player);
            Managers.SoundManager.Instance?.PlaySFX("powerup_collect");
            Destroy(gameObject);
        }

        /// <summary>
        /// Applies the power-up effect to the player.
        /// </summary>
        private void ApplyEffect(Player.PlayerController player)
        {
            switch (type)
            {
                case PowerUpType.Health:
                    player.HealPlayer(healAmount);
                    break;
                case PowerUpType.Shield:
                    player.ActivateShield();
                    break;
                case PowerUpType.RapidFire:
                    player.ActivateRapidFire();
                    break;
                case PowerUpType.SpreadShot:
                    player.ActivateSpreadShot();
                    break;
                case PowerUpType.ExtraLife:
                    player.AddLife();
                    break;
                case PowerUpType.ScoreBonus:
                    Managers.GameManager.Instance?.AddScore(scoreBonus);
                    break;
            }
        }
    }
}
