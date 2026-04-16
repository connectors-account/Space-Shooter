using SpaceShooter.Core;
using SpaceShooter.Managers;
using UnityEngine;

namespace SpaceShooter.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private int maxHealth = 6;
        [SerializeField] private float baseFireCooldown = 0.22f;

        private PoolManager _pool;
        private EnemySpawner _spawner;
        private Camera _camera;

        private int _health;
        private float _nextShotTime;
        private float _rapidFireUntil;
        private float _shieldUntil;
        private float _spreadShotUntil;

        public event System.Action<int, int> HealthChanged;
        public event System.Action<bool, bool, bool> PowerStateChanged;
        public event System.Action PlayerDied;

        public bool IsShielded => Time.time < _shieldUntil;

        public void Setup(PoolManager pool, EnemySpawner spawner)
        {
            _pool = pool;
            _spawner = spawner;
            _camera = Camera.main;
            _health = maxHealth;
            HealthChanged?.Invoke(_health, maxHealth);
            BroadcastPowerState();
        }

        private void Awake()
        {
            gameObject.tag = "Player";
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Update()
        {
            HandleMovement();
            HandleShooting();
            BroadcastPowerState();
        }

        private void HandleMovement()
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            var input = new Vector2(x, y).normalized;
            transform.position += (Vector3)input * (moveSpeed * Time.deltaTime);

            if (_camera == null)
            {
                return;
            }

            var p = transform.position;
            p.x = Mathf.Clamp(p.x, -8.8f, 8.8f);
            p.y = Mathf.Clamp(p.y, -4.6f, 4.6f);
            transform.position = p;
        }

        private void HandleShooting()
        {
            var firePressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
            if (!firePressed || Time.time < _nextShotTime)
            {
                return;
            }

            var cooldown = Time.time < _rapidFireUntil ? baseFireCooldown * 0.45f : baseFireCooldown;
            _nextShotTime = Time.time + cooldown;

            if (Time.time < _spreadShotUntil)
            {
                FireSpread();
            }
            else
            {
                FireOne(Vector2.up);
            }

            AudioManager.Instance?.PlayShoot();
        }

        private void FireOne(Vector2 direction)
        {
            var bulletObj = _pool.Spawn(GameBootstrap.PlayerBulletPool, transform.position + Vector3.up * 0.7f, Quaternion.identity);
            if (bulletObj == null)
            {
                return;
            }

            var projectile = bulletObj.GetComponent<Projectile>();
            projectile.Fire(direction, true, 15f, 1);
        }

        private void FireSpread()
        {
            var dirs = new[]
            {
                Quaternion.Euler(0f, 0f, -15f) * Vector2.up,
                Vector2.up,
                Quaternion.Euler(0f, 0f, 15f) * Vector2.up
            };

            foreach (var direction in dirs)
            {
                FireOne(direction);
            }
        }

        public void TakeDamage(int amount)
        {
            if (IsShielded)
            {
                return;
            }

            _health = Mathf.Max(0, _health - amount);
            HealthChanged?.Invoke(_health, maxHealth);
            AudioManager.Instance?.PlayHit();

            if (_health > 0)
            {
                return;
            }

            _spawner.StopSpawner();
            PlayerDied?.Invoke();
        }

        public void ApplyPowerUp(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.RapidFire:
                    _rapidFireUntil = Time.time + 8f;
                    break;
                case PowerUpType.Shield:
                    _shieldUntil = Time.time + 8f;
                    break;
                case PowerUpType.HealthRestore:
                    _health = Mathf.Min(maxHealth, _health + 2);
                    HealthChanged?.Invoke(_health, maxHealth);
                    break;
                case PowerUpType.SpreadShot:
                    _spreadShotUntil = Time.time + 10f;
                    break;
            }

            AudioManager.Instance?.PlayPowerUp();
            BroadcastPowerState();
        }

        private void BroadcastPowerState()
        {
            PowerStateChanged?.Invoke(
                Time.time < _rapidFireUntil,
                Time.time < _shieldUntil,
                Time.time < _spreadShotUntil
            );
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var projectile = other.GetComponent<Projectile>();
            if (projectile != null && !projectile.FromPlayer)
            {
                TakeDamage(projectile.Damage);
                other.GetComponent<PooledIdentity>()?.ReturnSelfToPool();
                return;
            }

            var pickup = other.GetComponent<PowerUpPickup>();
            if (pickup != null)
            {
                ApplyPowerUp(pickup.Type);
                other.GetComponent<PooledIdentity>()?.ReturnSelfToPool();
            }
        }
    }
}
