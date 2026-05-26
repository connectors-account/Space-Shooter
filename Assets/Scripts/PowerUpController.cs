using UnityEngine;

/// <summary>
/// Collectible power-up that drifts downward and applies an effect
/// when the player picks it up.
/// Requires: Collider2D (trigger), SpriteRenderer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PowerUpController : MonoBehaviour
{
    // ── Power-up Types ──────────────────────────────────────────────────
    public enum PowerUpType
    {
        WeaponUpgrade,
        Shield,
        HealthRestore,
        RapidFire,
        ScoreBonus
    }

    [Header("Power-Up")]
    [SerializeField] private PowerUpType type = PowerUpType.WeaponUpgrade;

    [Header("Movement")]
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float bobAmplitude = 0.3f;
    [SerializeField] private float bobFrequency = 3f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 10f;

    [Header("Effects")]
    [SerializeField] private float rapidFireDuration = 5f;
    [SerializeField] private float rapidFireMultiplier = 2f;
    [SerializeField] private int healAmount = 50;
    [SerializeField] private int scoreBonusAmount = 500;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    // ── Internals ───────────────────────────────────────────────────────
    private Rigidbody2D _rb;
    private float _startY;
    private float _aliveTime;

    // ── Color mapping for visual identification ─────────────────────────
    private static readonly Color[] TypeColors = new Color[]
    {
        new Color(1f, 0.5f, 0f),  // WeaponUpgrade  — orange
        new Color(0.3f, 0.8f, 1f), // Shield         — light blue
        new Color(0f, 1f, 0.3f),   // HealthRestore  — green
        new Color(1f, 1f, 0f),     // RapidFire      — yellow
        new Color(1f, 0f, 1f)      // ScoreBonus     — magenta
    };

    // ────────────────────────────────────────────────────────────────────
    // Unity Lifecycle
    // ────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;

        // Color tint based on type
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = TypeColors[(int)type];
    }

    private void Start()
    {
        _startY = transform.position.y;
        gameObject.tag = "PowerUp";
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        _aliveTime += Time.fixedDeltaTime;

        // Bob side-to-side while falling
        float bobOffset = Mathf.Sin(_aliveTime * bobFrequency) * bobAmplitude;
        _rb.linearVelocity = new Vector2(bobOffset, -fallSpeed);
    }

    // ────────────────────────────────────────────────────────────────────
    // Collection
    // ────────────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        HealthSystem health = other.GetComponent<HealthSystem>();

        if (player == null) return;

        ApplyEffect(player, health);

        // Play sound at position (survives object destruction)
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 0.7f);

        Destroy(gameObject);
    }

    // ────────────────────────────────────────────────────────────────────
    // Effects
    // ────────────────────────────────────────────────────────────────────
    private void ApplyEffect(PlayerController player, HealthSystem health)
    {
        switch (type)
        {
            case PowerUpType.WeaponUpgrade:
                player.UpgradeWeapon();
                break;

            case PowerUpType.Shield:
                player.HasShield = true;
                break;

            case PowerUpType.HealthRestore:
                if (health != null) health.Heal(healAmount);
                break;

            case PowerUpType.RapidFire:
                player.FireRateMultiplier = rapidFireMultiplier;
                // Reset after duration
                player.StartCoroutine(ResetRapidFire(player));
                break;

            case PowerUpType.ScoreBonus:
                GameManager.Instance?.AddScore(scoreBonusAmount);
                break;
        }
    }

    private System.Collections.IEnumerator ResetRapidFire(PlayerController player)
    {
        yield return new WaitForSeconds(rapidFireDuration);
        if (player != null)
            player.FireRateMultiplier = 1f;
    }
}
