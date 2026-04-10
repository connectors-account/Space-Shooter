using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float maxLifetime = 2.5f;
    [SerializeField] private int damage = 10;

    private ObjectPool _originPool;
    private Vector2 _direction = Vector2.up;
    private float _lifeTimer;

    public int Damage => damage;

    private void OnEnable()
    {
        _lifeTimer = maxLifetime;
    }

    public void Initialize(ObjectPool originPool, Vector2 direction)
    {
        _originPool = originPool;
        _direction = direction.normalized;
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
