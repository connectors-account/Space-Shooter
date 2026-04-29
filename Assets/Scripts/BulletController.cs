using UnityEngine;

/// <summary>
/// Moves bullet in a direction and stores owner/damage data.
/// </summary>
public class BulletController : MonoBehaviour
{
    [SerializeField] private bool isPlayerBullet = true;
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float lifeTime = 4f;

    private Vector2 direction;

    public bool IsPlayerBullet => isPlayerBullet;
    public int Damage => damage;

    private void Awake()
    {
        // Player bullets move up, enemy bullets move down by default.
        direction = isPlayerBullet ? Vector2.up : Vector2.down;
    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public void Configure(bool fromPlayer, float bulletSpeed, int bulletDamage)
    {
        isPlayerBullet = fromPlayer;
        speed = bulletSpeed;
        damage = bulletDamage;
        direction = isPlayerBullet ? Vector2.up : Vector2.down;
    }
}
