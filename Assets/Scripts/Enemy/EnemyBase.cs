// ============================================================
//  EnemyBase.cs  –  Health, score value, damage, death
// ============================================================
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int   maxHP      = 2;
    public int   scoreValue = 100;
    public float powerUpDropChance = 0.15f;   // 0–1

    public bool IsBoss { get; set; }

    protected int _hp;
    SpriteRenderer _sr;
    Color          _baseColor;

    protected virtual void Awake()
    {
        _hp = maxHP;
        _sr = GetComponent<SpriteRenderer>();
        if (_sr) _baseColor = _sr.color;
    }

    // ── Public API ───────────────────────────────────────────

    public void TakeDamage(int dmg)
    {
        _hp -= dmg;
        StartCoroutine(FlashRed());

        if (IsBoss)
            UIManager.Instance?.RefreshBossHP(_hp, maxHP);

        if (_hp <= 0)
            Die();
    }

    // ── Death ────────────────────────────────────────────────

    protected virtual void Die()
    {
        ScoreManager.Instance?.Add(scoreValue);
        AudioManager.Instance?.PlayExplosion();
        Explosion.Spawn(transform.position, IsBoss);

        if (Random.value < powerUpDropChance)
            PowerUpSpawner.Instance?.DropAt(transform.position);

        Destroy(gameObject);
    }

    // ── Hit flash ────────────────────────────────────────────

    System.Collections.IEnumerator FlashRed()
    {
        if (_sr) _sr.color = Color.red;
        yield return new WaitForSeconds(0.06f);
        if (_sr) _sr.color = _baseColor;
    }

    // ── Collision ────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            var b = other.GetComponent<Bullet>();
            TakeDamage(b ? b.damage : 1);
            b?.Despawn();
        }
    }
}
