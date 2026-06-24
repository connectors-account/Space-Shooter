using UnityEngine;

namespace SpaceShooter
{
    public enum PowerUpType
    {
        Health,
        WeaponUpgrade,
        Shield
    }

    /// <summary>
    /// A floating power-up pickup. Drifts down the screen and applies its
    /// effect to the player on contact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpController : MonoBehaviour
    {
        [Header("Power-up")]
        [SerializeField] private PowerUpType type = PowerUpType.Health;
        [SerializeField] private float fallSpeed = 2f;

        [Header("Effect Amounts")]
        [SerializeField] private float healthAmount = 35f;
        [SerializeField] private float shieldDuration = 5f;

        private float despawnY;

        private void Start()
        {
            if (Camera.main != null)
            {
                despawnY = Camera.main.ViewportToWorldPoint(Vector3.zero).y - 2f;
            }
            else
            {
                despawnY = -12f;
            }
        }

        private void Update()
        {
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime, Space.World);
            // Gentle spin for visual appeal.
            transform.Rotate(0f, 0f, 45f * Time.deltaTime);

            if (transform.position.y < despawnY)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyEffect(player);
                Destroy(gameObject);
            }
        }

        private void ApplyEffect(PlayerController player)
        {
            switch (type)
            {
                case PowerUpType.Health:
                    player.AddHealth(healthAmount);
                    break;
                case PowerUpType.WeaponUpgrade:
                    player.UpgradeWeapon();
                    break;
                case PowerUpType.Shield:
                    player.ActivateShield(shieldDuration);
                    break;
            }

            UIManager.Instance?.ShowPowerUpText(type.ToString());
        }
    }
}
