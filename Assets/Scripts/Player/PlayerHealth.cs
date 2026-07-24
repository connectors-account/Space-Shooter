// ============================================================
//  PlayerHealth.cs  –  HP, shield, invincibility frames, death
// ============================================================
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int   maxHP          = 5;
    public float invincibleTime = 1.5f;   // seconds of i-frames after a hit

    [Header("Shield")]
    public GameObject shieldVisual;       // assign a child circle sprite

    public int  HP      { get; private set; }
    public bool HasShield { get; private set; }

    bool          _invincible;
    SpriteRenderer _sr;

    void Start()
    {
        HP  = maxHP;
        _sr = GetComponent<SpriteRenderer>();
        if (shieldVisual) shieldVisual.SetActive(false);
        UIManager.Instance?.RefreshHP(HP, maxHP);
    }

    // ── Public API ───────────────────────────────────────────

    public void TakeDamage(int amount)
    {
        if (_invincible) return;

        if (HasShield)
        {
            RemoveShield();
            AudioManager.Instance?.PlayHit();
            return;
        }

        HP -= amount;
        HP  = Mathf.Max(HP, 0);
        AudioManager.Instance?.PlayHit();
        ScoreManager.Instance?.BreakStreak();
        CameraShake.Instance?.Shake(0.15f, 0.2f);
        UIManager.Instance?.RefreshHP(HP, maxHP);

        if (HP <= 0)
            Die();
        else
            StartCoroutine(InvincibilityFlash());
    }

    public void Heal(int amount)
    {
        HP = Mathf.Min(HP + amount, maxHP);
        UIManager.Instance?.RefreshHP(HP, maxHP);
    }

    public void GrantShield(float duration)
    {
        HasShield = true;
        if (shieldVisual) shieldVisual.SetActive(true);
        StartCoroutine(ShieldTimeout(duration));
    }

    // ── Private ──────────────────────────────────────────────

    void RemoveShield()
    {
        HasShield = false;
        if (shieldVisual) shieldVisual.SetActive(false);
    }

    void Die()
    {
        Explosion.Spawn(transform.position, large: true);
        AudioManager.Instance?.PlayExplosion();
        GameManager.Instance?.OnPlayerDied();
        Destroy(gameObject);
    }

    IEnumerator InvincibilityFlash()
    {
        _invincible = true;
        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            if (_sr) _sr.enabled = !_sr.enabled;
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        if (_sr) _sr.enabled = true;
        _invincible = false;
    }

    IEnumerator ShieldTimeout(float dur)
    {
        yield return new WaitForSeconds(dur);
        RemoveShield();
    }

    // ── Collision ────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            var b = other.GetComponent<Bullet>();
            TakeDamage(b ? b.damage : 1);
            b?.Despawn();
        }
        else if (other.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }
}
