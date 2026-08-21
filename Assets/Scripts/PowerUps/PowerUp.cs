using UnityEngine;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// A collectible power-up pickup. Bobs while drifting slowly downward,
    /// is collected on trigger with the player, and self-destructs after 8s.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PowerUp : MonoBehaviour
    {
        [Header("Type")]
        [SerializeField] private PowerUpType type = PowerUpType.Shield;

        [Header("Motion")]
        [SerializeField] private float driftSpeed = 1.2f;
        [SerializeField] private float bobAmplitude = 0.25f;
        [SerializeField] private float bobFrequency = 3f;

        [Header("Lifetime")]
        [SerializeField] private float lifetime = 8f;

        private float _bobTimer;
        private float _lifeTimer;

        public PowerUpType Type => type;

        private void OnEnable()
        {
            _bobTimer = 0f;
            _lifeTimer = 0f;
        }

        private void Update()
        {
            _bobTimer += Time.deltaTime;
            _lifeTimer += Time.deltaTime;

            // Downward drift with horizontal bob.
            Vector3 pos = transform.position;
            pos.y -= driftSpeed * Time.deltaTime;
            pos.x += Mathf.Cos(_bobTimer * bobFrequency) * bobAmplitude * Time.deltaTime * 10f;
            transform.position = pos;

            if (_lifeTimer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var powerUpHandler = other.GetComponent<PlayerPowerUp>();
            if (powerUpHandler != null)
            {
                powerUpHandler.Activate(type);
                Destroy(gameObject);
            }
        }
    }
}
