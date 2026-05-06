using UnityEngine;

public enum PowerUpType
{
    RapidFire,
    Shield,
    Health
}

[RequireComponent(typeof(Collider2D))]
public class PowerUpController : MonoBehaviour
{
    [SerializeField] private PowerUpType powerUpType = PowerUpType.RapidFire;
    [SerializeField] private float moveSpeed = 2f;

    private void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

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
            case PowerUpType.RapidFire:
                player.ActivateRapidFire();
                break;

            case PowerUpType.Shield:
                player.ActivateShield();
                break;

            case PowerUpType.Health:
                player.Heal(1);
                break;
        }

        AudioManager.Instance?.PlaySfx(AudioManager.Instance.PowerUpClip);
        Destroy(gameObject);
    }
}
