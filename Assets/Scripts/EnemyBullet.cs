using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private int defaultDamage = 10;

    private ObjectPool _originPool;
    private Vector2 _direction = Vector2.down;
    private float _lifeTimer;
    private int _damage;

    public void Initialize(ObjectPool originPool, Vector2 direction, int damageOverride = -1)
    {
        _originPool = originPool;
        _direction = direction.normalized;
        _damage = damageOverride > 0 ? damageOverride : defaultDamage;
        _lifeTimer = maxLifetime;
    }

    private void OnEnable()
    {
        _lifeTimer = maxLifetime;
    }

    private void Update()
    {
        transform.position += (Vector3)(_direction * speed * Time.deltaTime);
        _lifeTimer -= Time.deltaTime;

        if (_lifeTimer <= 0f)
        {
            Release();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            player.TakeDamage(_damage);
            Release();
        }
    }

    public void Release()
    {
        if (_originPool != null)
        {
            _originPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnBecameInvisible()
    {
        Release();
    }
}
