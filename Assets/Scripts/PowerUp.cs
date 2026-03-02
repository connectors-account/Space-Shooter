using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        WeaponUpgrade,
        Health,
        Shield
    }

    [Header("Power-Up Settings")]
    public PowerUpType type = PowerUpType.WeaponUpgrade;
    public float moveSpeed = 2f;
    public float lifetime = 10f;

    [Header("Effects")]
    public int healthAmount = 2;
    public float shieldDuration = 5f;

    [Header("Audio")]
    public AudioClip pickupSound;

    [Header("Visual")]
    public float bobAmplitude = 0.3f;
    public float bobSpeed = 2f;

    private Vector3 startPosition;
    private float bobTimer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        startPosition = transform.position;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused())
            return;

        // Move downward
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        // Bob up and down
        bobTimer += Time.deltaTime * bobSpeed;
        float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
        Vector3 bobPosition = transform.position;
        bobPosition.x = startPosition.x + bobOffset;
        startPosition = new Vector3(transform.position.x - bobOffset, startPosition.y - moveSpeed * Time.deltaTime, startPosition.z);

        // Check bounds
        if (transform.position.y < -6f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyPowerUp(player);
            }

            PlaySound(pickupSound);
            Destroy(gameObject);
        }
    }

    void ApplyPowerUp(PlayerController player)
    {
        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                player.UpgradeWeapon();
                break;

            case PowerUpType.Health:
                player.Heal(healthAmount);
                break;

            case PowerUpType.Shield:
                player.ActivateShield(shieldDuration);
                break;
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position);
        }
    }
}
