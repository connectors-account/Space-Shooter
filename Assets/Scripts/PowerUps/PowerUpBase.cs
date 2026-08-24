using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Data;
using SpaceShooter.Utilities;
using SpaceShooter.Player;
using SpaceShooter.Enemy;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// A collectible power-up. Drifts downward while bobbing and rotating, and applies
    /// its effect to the player on contact.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpBase : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PowerUpData data;

        [Header("Motion")]
        [SerializeField] private float driftSpeed = 1.5f;
        [SerializeField] private float bobAmplitude = 0.25f;
        [SerializeField] private float bobFrequency = 3f;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Effect Defaults")]
        [SerializeField] private int healthPackAmount = 30;
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private float speedBoostMultiplier = 1.5f;
        [SerializeField] private float speedBoostDuration = 5f;

        private SpriteRenderer _spriteRenderer;
        private float _startX;
        private float _spawnTime;
        private float _bottomBound;

        public PowerUpData Data => data;

        public void SetData(PowerUpData powerUpData)
        {
            data = powerUpData;
            ApplyVisuals();
        }

        private void Awake()
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        private void Start()
        {
            _startX = transform.position.x;
            _spawnTime = Time.time;

            Camera cam = Camera.main;
            _bottomBound = cam != null ? cam.ViewportToWorldPoint(Vector3.zero).y - 1f : -10f;

            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (_spriteRenderer != null && data != null)
            {
                if (data.icon != null)
                {
                    _spriteRenderer.sprite = data.icon;
                }
                _spriteRenderer.color = data.glowColor;
            }
        }

        private void Update()
        {
            float t = Time.time - _spawnTime;

            Vector3 pos = transform.position;
            pos.y -= driftSpeed * Time.deltaTime;
            pos.x = _startX + Mathf.Sin(t * bobFrequency) * bobAmplitude;
            transform.position = pos;

            // Spin the visual child (keep the collider upright by rotating the sprite only).
            if (_spriteRenderer != null)
            {
                _spriteRenderer.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }

            if (transform.position.y < _bottomBound)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(Constants.Tags.Player))
            {
                return;
            }

            ApplyEffect(other.gameObject);

            if (AudioManager.HasInstance)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.powerUpSFX);
            }

            if (data != null && GameManager.HasInstance && data.scoreValue > 0)
            {
                GameManager.Instance.AddScore(data.scoreValue);
            }

            Destroy(gameObject);
        }

        private void ApplyEffect(GameObject player)
        {
            PowerUpType type = data != null ? data.type : PowerUpType.WeaponUpgrade;

            switch (type)
            {
                case PowerUpType.WeaponUpgrade:
                {
                    var shooter = player.GetComponentInChildren<PlayerShooter>();
                    if (shooter == null) shooter = player.GetComponent<PlayerShooter>();
                    shooter?.UpgradeWeapon();
                    break;
                }

                case PowerUpType.Shield:
                {
                    var hp = player.GetComponent<PlayerHealth>();
                    float dur = (data != null && data.duration > 0f) ? data.duration : shieldDuration;
                    hp?.ActivateShield(dur);
                    break;
                }

                case PowerUpType.HealthPack:
                {
                    var hp = player.GetComponent<PlayerHealth>();
                    int amount = (data != null && data.magnitude > 0f) ? Mathf.RoundToInt(data.magnitude) : healthPackAmount;
                    hp?.Heal(amount);
                    break;
                }

                case PowerUpType.SpeedBoost:
                {
                    var controller = player.GetComponent<PlayerController>();
                    float mult = (data != null && data.magnitude > 0f) ? data.magnitude : speedBoostMultiplier;
                    float dur = (data != null && data.duration > 0f) ? data.duration : speedBoostDuration;
                    controller?.ApplySpeedBoost(mult, dur);
                    break;
                }

                case PowerUpType.BombClear:
                    ClearAllEnemies();
                    break;
            }
        }

        private void ClearAllEnemies()
        {
            var enemies = GameObject.FindObjectsOfType<EnemyHealth>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(int.MaxValue);
                }
            }
        }
    }
}
