using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public enum PowerUpType
    {
        RapidFire,
        Shield,
        HealthRestore,
        SpreadShot
    }

    [RequireComponent(typeof(Collider2D))]
    public class PowerUpPickup : MonoBehaviour, IPoolable
    {
        [SerializeField] private float moveSpeed = 1.8f;
        [SerializeField] private float lifeTime = 10f;

        private float _expireTime;
        private PooledIdentity _identity;
        private SpriteRenderer _spriteRenderer;

        public PowerUpType Type { get; private set; }

        private void Awake()
        {
            _identity = GetComponent<PooledIdentity>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            transform.position += Vector3.down * (moveSpeed * Time.deltaTime);
            transform.Rotate(0f, 0f, 60f * Time.deltaTime);

            if (Time.time >= _expireTime || transform.position.y < -7f)
            {
                _identity.ReturnSelfToPool();
            }
        }

        public void Configure(PowerUpType type, Color color)
        {
            Type = type;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }
        }

        public void OnSpawned()
        {
            _expireTime = Time.time + lifeTime;
        }

        public void OnReturnedToPool()
        {
        }
    }
}
