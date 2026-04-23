using SpaceShooter.Player;
using UnityEngine;

namespace SpaceShooter.Powerups
{
    public enum PowerUpType
    {
        RapidFire,
        Shield,
        HealthRestore
    }

    [RequireComponent(typeof(Collider2D))]
    public class PowerUp : MonoBehaviour
    {
        [SerializeField] private PowerUpType powerUpType;
        [SerializeField] private float fallSpeed = 2f;
        [SerializeField] private float duration = 6f;
        [SerializeField] private int healthRestoreAmount = 25;
        [SerializeField] private float lifeTime = 10f;

        private Camera cam;

        public PowerUpType Type => powerUpType;

        public void Initialize(PowerUpType type)
        {
            powerUpType = type;
        }

        private void Awake()
        {
            cam = Camera.main;
            gameObject.layer = Core.GameLayers.GetLayerOrDefault(Core.GameLayers.PowerUp);
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            if (cam != null && transform.position.y < Core.ScreenBounds.MinWorld(cam).y - 0.8f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null) return;

            player.ApplyPowerup(powerUpType, duration, healthRestoreAmount);
            Destroy(gameObject);
        }
    }
}
