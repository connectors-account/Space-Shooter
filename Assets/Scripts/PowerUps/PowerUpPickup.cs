using UnityEngine;

namespace SpaceShooter.PowerUps
{
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private PowerUpType powerUpType;
        [SerializeField] private float downwardSpeed = 2.2f;
        [SerializeField] private float lifeTime = 10f;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(Vector2.down * (downwardSpeed * Time.deltaTime), Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && other.TryGetComponent(out PlayerPowerUpController powerUpController))
            {
                powerUpController.ApplyPowerUp(powerUpType);
                Destroy(gameObject);
            }
            else if (other.CompareTag("Bounds"))
            {
                Destroy(gameObject);
            }
        }
    }
}
