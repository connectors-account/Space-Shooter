using SpaceShooter.Core;
using SpaceShooter.Visual;
using UnityEngine;

namespace SpaceShooter.Projectiles
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        private ObjectPoolManager _pool;
        private float _speed;
        private int _damage;
        private Vector2 _direction;
        private Faction _ownerFaction;
        private float _lifetime;

        public void Initialize(ObjectPoolManager pool, Faction ownerFaction, Vector2 direction, float speed, int damage, float lifetime = 5f)
        {
            _pool = pool;
            _ownerFaction = ownerFaction;
            _direction = direction.normalized;
            _speed = speed;
            _damage = damage;
            _lifetime = lifetime;
        }

        private void OnEnable()
        {
            _lifetime = Mathf.Max(0.2f, _lifetime);
        }

        private void Update()
        {
            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f || Mathf.Abs(transform.position.x) > 12f || Mathf.Abs(transform.position.y) > 7f)
            {
                _pool.Release(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out IDamageable damageable) || damageable.Faction == _ownerFaction)
            {
                return;
            }

            damageable.ApplyDamage(_damage, transform.position);
            EffectManager.Instance?.SpawnHit(transform.position);
            Sound.SoundManager.Instance?.PlaySfx("hit");
            _pool.Release(gameObject);
        }
    }
}
