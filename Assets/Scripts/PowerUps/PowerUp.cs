// ============================================================
//  PowerUp.cs  –  Collectible power-up item
//  Types: RapidFire, TripleShot, Shield, Heal, SpeedBoost
// ============================================================
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type { RapidFire, TripleShot, Shield, Heal, SpeedBoost }

    [Header("Config")]
    public Type   powerUpType  = Type.RapidFire;
    public float  fallSpeed    = 2.5f;
    public float  duration     = 8f;    // for timed effects
    public int    healAmount   = 2;

    // ── Visuals ──────────────────────────────────────────────

    static readonly Color[] TypeColors =
    {
        new Color(1f, 0.85f, 0f),      // RapidFire  – yellow
        new Color(0f, 0.8f, 1f),       // TripleShot – cyan
        new Color(0.4f, 0.4f, 1f),     // Shield     – blue
        new Color(0.2f, 1f, 0.4f),     // Heal       – green
        new Color(1f, 0.5f, 0f)        // SpeedBoost – orange
    };

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = TypeColors[(int)powerUpType];
    }

    void Update() => transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);

    // ── Pickup ───────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        ApplyEffect(other.gameObject);
        AudioManager.Instance?.PlayPowerUp();
        UIManager.Instance?.ShowPowerUpText(powerUpType.ToString());
        Destroy(gameObject);
    }

    void ApplyEffect(GameObject player)
    {
        switch (powerUpType)
        {
            case Type.RapidFire:
                player.GetComponent<PlayerShooter>()?.ApplyRapidFire(duration);
                break;

            case Type.TripleShot:
                var sh = player.GetComponent<PlayerShooter>();
                sh?.SetFireMode(PlayerShooter.FireMode.Triple);
                // Auto-revert after duration
                LeanTween.delayedCall(duration, () =>
                {
                    if (sh) sh.SetFireMode(PlayerShooter.FireMode.Single);
                });
                break;

            case Type.Shield:
                player.GetComponent<PlayerHealth>()?.GrantShield(duration);
                break;

            case Type.Heal:
                player.GetComponent<PlayerHealth>()?.Heal(healAmount);
                break;

            case Type.SpeedBoost:
                player.GetComponent<PlayerController>()?.ApplySpeedBoost(duration);
                break;
        }
    }

    void OnBecameInvisible() => Destroy(gameObject);
}
