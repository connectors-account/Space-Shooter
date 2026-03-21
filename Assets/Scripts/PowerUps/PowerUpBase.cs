using UnityEngine;

/// <summary>
/// Base class for all power-up pickups. Drifts downward and is collected on contact with player.
/// </summary>
public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Power-Up Movement")]
    public float fallSpeed = 2f;
    public float bobAmplitude = 0.3f;
    public float bobFrequency = 2f;

    private float startY;
    private float timeOffset;

    protected virtual void Start()
    {
        startY = transform.position.y;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    protected virtual void Update()
    {
        // Drift downward with bobbing motion
        float bob = Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);

        Vector3 pos = transform.position;
        pos.x += Mathf.Cos((Time.time + timeOffset) * bobFrequency) * 0.01f;
        transform.position = pos;

        // Destroy if off screen
        if (transform.position.y < -7f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            ApplyPowerUp(player);
            AudioManager.Instance?.PlaySFX("PowerUp");
            Destroy(gameObject);
        }
    }

    /// <summary>Override to define power-up effect.</summary>
    protected abstract void ApplyPowerUp(PlayerController player);
}
