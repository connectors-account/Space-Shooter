// ============================================================================
// EnemyBase.cs — Base class for all enemies
// ============================================================================
using UnityEngine;
using System;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected int maxHealth = 3;
    [SerializeField] protected int scoreValue = 100;
    [SerializeField] protected int contactDamage = 1;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 2f;
    [SerializeField] protected MovementPattern movementPattern = MovementPattern.StraightDown;

    [Header("Effects")]
    [SerializeField] protected GameObject explosionPrefab;
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip deathSound;
    [SerializeField] protected GameObject deathDropPrefab; // optional specific drop

    // Events
    public event Action<EnemyBase> OnEnemyDestroyed;

    // Runtime
    protected int currentHealth;
    protected SpriteRenderer spriteRenderer;
    protected bool isDead;

    // Properties
    public int ScoreValue => scoreValue;
    public bool IsDead => isDead;

    // =========================================================================
    public enum MovementPattern
    {
        StraightDown,
        SineWave,
        ZigZag,
        DiagonalLeft,
        DiagonalRight,
        Hovering,   // moves down then stops and hovers
        Kamikaze    // aims at player
    }

    // =========================================================================
    protected virtual void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        // Apply difficulty scaling
        if (GameManager.Instance != null)
        {
            float diff = GameManager.Instance.GetDifficultyMultiplier();
            maxHealth = Mathf.RoundToInt(maxHealth * (1f + (diff - 1f) * 0.5f));
            currentHealth = maxHealth;
            moveSpeed *= (1f + (diff - 1f) * 0.3f);
        }
    }

    protected virtual void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing) return;
        if (isDead) return;

        Move();
        CheckBounds();
    }

    // =========================================================================
    // Movement
    // =========================================================================
    protected virtual void Move()
    {
        Vector3 movement = Vector3.zero;

        switch (movementPattern)
        {
            case MovementPattern.StraightDown:
                movement = Vector3.down * moveSpeed;
                break;

            case MovementPattern.SineWave:
                movement = Vector3.down * moveSpeed;
                movement.x = Mathf.Sin(Time.time * 3f + transform.position.y) * 2f;
                break;

            case MovementPattern.ZigZag:
                movement = Vector3.down * moveSpeed;
                movement.x = Mathf.PingPong(Time.time * 2f, 1f) * 4f - 2f;
                break;

            case MovementPattern.DiagonalLeft:
                movement = (Vector3.down + Vector3.left * 0.5f).normalized * moveSpeed;
                break;

            case MovementPattern.DiagonalRight:
                movement = (Vector3.down + Vector3.right * 0.5f).normalized * moveSpeed;
                break;

            case MovementPattern.Hovering:
                if (transform.position.y > 3f)
                    movement = Vector3.down * moveSpeed;
                else
                    movement.x = Mathf.Sin(Time.time * 2f) * moveSpeed * 0.5f;
                break;

            case MovementPattern.Kamikaze:
                GameObject player = GameManager.Instance?.PlayerShip;
                if (player != null)
                {
                    Vector3 dir = (player.transform.position - transform.position).normalized;
                    movement = dir * moveSpeed * 1.5f;
                }
                else
                {
                    movement = Vector3.down * moveSpeed;
                }
                break;
        }

        transform.Translate(movement * Time.deltaTime, Space.World);
    }

    // =========================================================================
    // Damage
    // =========================================================================
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Flash white
        if (spriteRenderer != null)
        {
            StartCoroutine(FlashWhite());
        }

        if (SoundManager.Instance != null && hitSound != null)
            SoundManager.Instance.PlaySFX(hitSound, 0.4f);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // Score
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        // Death sound
        if (SoundManager.Instance != null && deathSound != null)
            SoundManager.Instance.PlaySFX(deathSound, 0.6f);

        // Explosion effect
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        // Notify wave manager
        OnEnemyDestroyed?.Invoke(this);

        Destroy(gameObject);
    }

    // =========================================================================
    // Bounds
    // =========================================================================
    protected void CheckBounds()
    {
        if (transform.position.y < -7f || transform.position.y > 10f ||
            Mathf.Abs(transform.position.x) > 8f)
        {
            OnEnemyDestroyed?.Invoke(this);
            Destroy(gameObject);
        }
    }

    // =========================================================================
    // Visual Effects
    // =========================================================================
    private System.Collections.IEnumerator FlashWhite()
    {
        if (spriteRenderer == null) yield break;
        Color orig = spriteRenderer.color;
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.05f);
        if (spriteRenderer != null)
            spriteRenderer.color = orig;
    }

    // =========================================================================
    // Collision with Player
    // =========================================================================
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(contactDamage);
            TakeDamage(999); // Kamikaze: enemy also dies on contact
        }
        else if (other.CompareTag("PlayerBullet"))
        {
            Bullet b = other.GetComponent<Bullet>();
            int dmg = b != null ? b.Damage : 1;
            TakeDamage(dmg);
            Destroy(other.gameObject);
        }
    }
}
