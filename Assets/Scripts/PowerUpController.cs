using UnityEngine;

public class PowerUpController : MonoBehaviour
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        HealthRestore
    }

    [SerializeField] private PowerUpType powerUpType;
    [SerializeField] private float driftSpeed = 2.3f;
    [SerializeField] private float rotateSpeed = 110f;
    [SerializeField] private float lifeTime = 8f;

    private void Start()
    {
        gameObject.tag = "PowerUp";
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += Vector3.down * driftSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);

        if (transform.position.y < -6.5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        switch (powerUpType)
        {
            case PowerUpType.Shield:
                player.ActivateShield();
                break;
            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                break;
            case PowerUpType.HealthRestore:
                player.Heal(1);
                break;
        }

        AudioManager.Instance?.PlayPowerUp();
        Destroy(gameObject);
    }
}
