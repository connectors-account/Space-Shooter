using UnityEngine;

/// <summary>
/// Power-up pickup. Attach to a prefab with Sprite + BoxCollider2D (isTrigger).
/// Tag as "PowerUp".
/// Create one prefab per type, or use random assignment.
/// </summary>
public class PowerUp : MonoBehaviour
{
    public enum Type { RapidFire, Shield, Health }

    [Header("Config")]
    public Type powerUpType = Type.RapidFire;
    public bool randomizeOnSpawn = true;
    public float fallSpeed = 2f;
    public float lifetime = 10f;

    void Start()
    {
        if (randomizeOnSpawn)
        {
            powerUpType = (Type)Random.Range(0, 3);
        }
        // Color-code the sprite
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (powerUpType)
            {
                case Type.RapidFire: sr.color = Color.yellow; break;
                case Type.Shield:    sr.color = new Color(0.3f, 0.7f, 1f); break;
                case Type.Health:    sr.color = Color.green; break;
            }
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        if (transform.position.y < -7f) Destroy(gameObject);
    }

    /// <summary>
    /// Called by PlayerController on pickup.
    /// </summary>
    public void Apply(PlayerController player)
    {
        SoundManager.Instance?.PlaySFX("PowerUp");
        switch (powerUpType)
        {
            case Type.RapidFire:
                player.ActivateRapidFire();
                break;
            case Type.Shield:
                player.ActivateShield();
                break;
            case Type.Health:
                player.Heal(40);
                break;
        }
    }
}
