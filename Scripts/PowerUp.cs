using UnityEngine;

/// <summary>
/// Handles falling power-up behavior and applies effects when collected by the player.
/// Attach this to the power-up prefab.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        RapidFire
    }

    [SerializeField] private PowerUpType powerUpType = PowerUpType.RapidFire;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float lifeTime = 8f;
    [SerializeField] private float rapidFireDuration = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime), Space.World);
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
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

        ApplyEffect(player);
        Destroy(gameObject);
    }

    private void ApplyEffect(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.RapidFire:
                player.ApplyRapidFire(rapidFireDuration);
                break;
        }
    }
}
