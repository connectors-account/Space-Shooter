using SpaceShooter.Audio;
using SpaceShooter.Combat;
using SpaceShooter.Core;
using SpaceShooter.Utils;
using UnityEngine;

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        private float _moveSpeed = 6f;
        private float _normalFireCooldown = 0.22f;
        private float _rapidFireCooldown = 0.09f;
        private float _nextFireTime;
        private float _rapidFireUntil;
        private float _shieldUntil;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            name = "Player";
            transform.position = new Vector3(0f, -3.8f, 0f);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = SpriteFactory.GetSprite(new Color(0.2f, 0.8f, 1f), ShapeType.Triangle, 42);

            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.45f;

            var rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.isKinematic = true;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            HandleMovement();
            HandleShooting();
            UpdateShieldVisual();
        }

        public bool IsShielded => Time.time < _shieldUntil;

        public void TakeDamage(int amount)
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            if (IsShielded)
            {
                return;
            }

            GameManager.Instance.DamagePlayer(amount);
        }

        public void ActivateShield(float duration)
        {
            _shieldUntil = Mathf.Max(_shieldUntil, Time.time + duration);
        }

        public void ActivateRapidFire(float duration)
        {
            _rapidFireUntil = Mathf.Max(_rapidFireUntil, Time.time + duration);
        }

        private void HandleMovement()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            var movement = new Vector3(horizontal, vertical, 0f).normalized;

            transform.position += movement * (_moveSpeed * Time.deltaTime);
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, -8.5f, 8.5f),
                Mathf.Clamp(transform.position.y, -4.6f, 4.6f),
                0f);
        }

        private void HandleShooting()
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                return;
            }

            var cooldown = Time.time < _rapidFireUntil ? _rapidFireCooldown : _normalFireCooldown;
            if (Time.time < _nextFireTime)
            {
                return;
            }

            _nextFireTime = Time.time + cooldown;
            SpawnBullet(new Vector3(transform.position.x, transform.position.y + 0.65f, 0f), Vector2.up, 12f, 1, true, new Color(0.4f, 1f, 1f));

            AudioManager.Instance?.PlayShoot();
        }

        private static void SpawnBullet(Vector3 position, Vector2 direction, float speed, int damage, bool playerBullet, Color color)
        {
            var bulletObject = new GameObject(playerBullet ? "PlayerBullet" : "EnemyBullet");
            bulletObject.transform.position = position;
            var bullet = bulletObject.AddComponent<Bullet>();
            bullet.Initialize(direction, speed, damage, playerBullet, color);
        }

        private void UpdateShieldVisual()
        {
            var renderer = GetComponent<SpriteRenderer>();
            renderer.color = IsShielded ? new Color(0.7f, 1f, 1f) : Color.white;
        }
    }
}
