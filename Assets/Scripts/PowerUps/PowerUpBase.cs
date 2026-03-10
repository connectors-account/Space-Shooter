using UnityEngine;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    public enum PowerUpType
    {
        WeaponUpgrade,
        Health,
        Shield,
        SpeedBoost,
        ExtraLife,
        ScoreMultiplier
    }

    public class PowerUpBase : MonoBehaviour
    {
        [Header("Power-Up Settings")]
        [SerializeField] protected PowerUpType powerUpType;
        [SerializeField] protected float floatSpeed = 2f;
        [SerializeField] protected float lifetime = 10f;
        [SerializeField] protected float bobAmplitude = 0.2f;
        [SerializeField] protected float bobFrequency = 2f;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Color flashColor = Color.white;
        [SerializeField] protected float flashDuration = 0.1f;

        protected Vector3 startPosition;
        protected float timeAlive;
        protected bool isCollected = false;

        public PowerUpType Type => powerUpType;

        protected virtual void Start()
        {
            startPosition = transform.position;
            Destroy(gameObject, lifetime);
            
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void Update()
        {
            timeAlive += Time.deltaTime;
            Move();
            Flash();
        }

        protected virtual void Move()
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= floatSpeed * Time.deltaTime;
            
            float bobOffset = Mathf.Sin(timeAlive * bobFrequency) * bobAmplitude;
            newPosition.x = startPosition.x + bobOffset;
            
            transform.position = newPosition;
            startPosition.y = newPosition.y;

            if (newPosition.y < -6f)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void Flash()
        {
            if (lifetime - timeAlive < 3f && spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(timeAlive * 5f, 1f);
                Color currentColor = spriteRenderer.color;
                currentColor.a = 0.5f + alpha * 0.5f;
                spriteRenderer.color = currentColor;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    ApplyEffect(player);
                    isCollected = true;
                    Managers.AudioManager.Instance?.PlaySound("PowerUp");
                    Destroy(gameObject);
                }
            }
        }

        protected virtual void ApplyEffect(PlayerController player)
        {
            // Override in derived classes
        }
    }
}
