using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 14f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private int damage = 1;

        private Vector2 _direction = Vector2.up;
        private bool _fromPlayer;
        private float _despawnTime;
        private PooledIdentity _identity;

        public int Damage => damage;
        public bool FromPlayer => _fromPlayer;

        private void Awake()
        {
            _identity = GetComponent<PooledIdentity>();
            var collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            if (Time.time >= _despawnTime || Mathf.Abs(transform.position.y) > 7f || Mathf.Abs(transform.position.x) > 11f)
            {
                _identity.ReturnSelfToPool();
            }
        }

        public void Fire(Vector2 direction, bool fromPlayer, float overrideSpeed, int overrideDamage)
        {
            _direction = direction.normalized;
            _fromPlayer = fromPlayer;
            speed = overrideSpeed;
            damage = overrideDamage;
            _despawnTime = Time.time + lifetime;
        }

        public void OnSpawned()
        {
            _despawnTime = Time.time + lifetime;
        }

        public void OnReturnedToPool()
        {
        }
    }
}
