using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1;
    public bool destroyOnContact = false;
    public string[] targetTags = { "Player" };

    [Header("Cooldown")]
    public float damageCooldown = 0.5f;

    private float lastDamageTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (Time.time - lastDamageTime >= damageCooldown)
        {
            TryDamage(other.gameObject);
        }
    }

    private void TryDamage(GameObject target)
    {
        foreach (string tag in targetTags)
        {
            if (target.CompareTag(tag))
            {
                // Check for player invincibility
                PlayerController player = target.GetComponent<PlayerController>();
                if (player != null && player.IsInvincible())
                {
                    return;
                }

                HealthSystem healthSystem = target.GetComponent<HealthSystem>();
                if (healthSystem != null)
                {
                    healthSystem.TakeDamage(damage);
                    lastDamageTime = Time.time;

                    if (destroyOnContact)
                    {
                        Destroy(gameObject);
                    }
                }
                break;
            }
        }
    }
}
