using SpaceShooter.Player;
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        HealthRestore
    }

    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpType powerUpType = PowerUpType.Shield;
        [SerializeField] private float value = 25f;
        [SerializeField] private float fallSpeed = 2.2f;
        [SerializeField] private float lifeTime = 10f;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.Translate(Vector3.down * (fallSpeed * Time.deltaTime), Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController playerController))
            {
                return;
            }

            playerController.ApplyPowerUp(powerUpType, value);
            Destroy(gameObject);
        }
    }
}
