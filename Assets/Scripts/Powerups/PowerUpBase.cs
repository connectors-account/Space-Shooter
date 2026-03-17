using UnityEngine;

namespace SpaceShooter.Core
{
    public enum PowerUpType
    {
        Health,
        Shield,
        RapidFire,
        TripleShot,
        ExtraLife,
        ScoreBonus,
        SpeedBoost
    }
    
    /// <summary>
    /// Base class for all power-ups
    /// </summary>
    public class PowerUpBase : MonoBehaviour
    {
        [Header("Power-Up Settings")]
        [SerializeField] protected PowerUpType powerUpType = PowerUpType.Health;
        [SerializeField] protected float effectDuration = 5f;
        [SerializeField] protected int effectValue = 25;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float lifetime = 10f;
        
        [Header("Visual Effects")]
        [SerializeField] protected float bobSpeed = 2f;
        [SerializeField] protected float bobAmount = 0.2f;
        [SerializeField] protected float rotationSpeed = 90f;
        [SerializeField] protected GameObject collectEffectPrefab;
        
        [Header("Audio")]
        [SerializeField] protected AudioClip collectSound;
        
        private Vector3 startPosition;
        private float spawnTime;
        
        protected virtual void Start()
        {
            startPosition = transform.position;
            spawnTime = Time.time;
            
            Destroy(gameObject, lifetime);
        }
        
        protected virtual void Update()
        {
            // Move downward
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            
            // Bob up and down
            float yOffset = Mathf.Sin((Time.time - spawnTime) * bobSpeed) * bobAmount;
            transform.position = new Vector3(transform.position.x, transform.position.y + yOffset * Time.deltaTime, transform.position.z);
            
            // Rotate
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // Destroy if off screen
            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
        
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    ApplyEffect(player);
                    PlayCollectEffects();
                    Destroy(gameObject);
                }
            }
        }
        
        protected virtual void ApplyEffect(PlayerController player)
        {
            switch (powerUpType)
            {
                case PowerUpType.Health:
                    player.Heal(effectValue);
                    break;
                    
                case PowerUpType.Shield:
                    player.ActivateShield(effectDuration);
                    break;
                    
                case PowerUpType.RapidFire:
                    player.ActivateRapidFire(effectDuration);
                    break;
                    
                case PowerUpType.TripleShot:
                    player.ActivateTripleShot(effectDuration);
                    break;
                    
                case PowerUpType.ExtraLife:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.GainLife();
                    }
                    break;
                    
                case PowerUpType.ScoreBonus:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddScore(effectValue);
                    }
                    break;
            }
        }
        
        protected void PlayCollectEffects()
        {
            if (collectEffectPrefab != null)
            {
                GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position, 0.5f);
            }
        }
    }
}
