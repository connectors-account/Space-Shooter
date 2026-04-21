using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Power-up pickup behavior and player effect application.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpController : MonoBehaviour
    {
        public enum PowerUpType
        {
            WeaponUpgrade,
            Shield,
            Health
        }

        [Header("Power-Up")]
        [SerializeField] private PowerUpType powerUpType = PowerUpType.WeaponUpgrade;
        [SerializeField] private int effectAmount = 1;
        [SerializeField] private float effectDuration = 6f;

        [Header("Motion")]
        [SerializeField] private float fallSpeed = 2f;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float lifetime = 8f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

            if (transform.position.y < -6.8f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                return;
            }

            player.ApplyPowerUp(powerUpType, effectAmount, effectDuration);
            Destroy(gameObject);
        }
    }
}
