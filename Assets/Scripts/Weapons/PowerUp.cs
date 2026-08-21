using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Player;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Floating power-up pickup. Drifts downward, rotates, and applies its effect to the player on contact.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PowerUp : MonoBehaviour
    {
        public enum PowerUpType { SpeedBoost, RapidFire, TripleShot, Shield, HealthPack, Bomb }

        [Header("Type")]
        [SerializeField] private PowerUpType type = PowerUpType.SpeedBoost;
        [SerializeField] private string poolTag = "PowerUp_Speed";

        [Header("Movement")]
        [SerializeField] private float fallSpeed = 2f;
        [SerializeField] private float rotationSpeed = 90f;

        [Header("Effect Values")]
        [SerializeField] private float timedDuration = 10f;
        [SerializeField] private float speedMultiplier = 1.6f;
        [SerializeField] private int healthAmount = 40;

        private Camera cam;

        public PowerUpType Type => type;

        private void Awake()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                float bottom = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, Mathf.Abs(cam.transform.position.z))).y;
                if (transform.position.y < bottom - 1f)
                {
                    ReturnToPool();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            ApplyEffect(other.gameObject);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("PowerUp");
            ReturnToPool();
        }

        private void ApplyEffect(GameObject playerObj)
        {
            PlayerController controller = playerObj.GetComponent<PlayerController>();
            PlayerShooter shooter = playerObj.GetComponent<PlayerShooter>();
            PlayerHealth health = playerObj.GetComponent<PlayerHealth>();

            switch (type)
            {
                case PowerUpType.SpeedBoost:
                    if (controller != null) controller.ApplySpeedBoost(speedMultiplier, timedDuration);
                    if (health != null) health.StartRegeneration(timedDuration);
                    break;

                case PowerUpType.RapidFire:
                    if (shooter != null) shooter.SetWeaponMode(PlayerShooter.WeaponMode.Rapid, timedDuration);
                    break;

                case PowerUpType.TripleShot:
                    if (shooter != null) shooter.SetWeaponMode(PlayerShooter.WeaponMode.Triple, timedDuration);
                    break;

                case PowerUpType.Shield:
                    if (health != null) health.ActivateShield();
                    break;

                case PowerUpType.HealthPack:
                    if (health != null) health.Heal(healthAmount);
                    break;

                case PowerUpType.Bomb:
                    DetonateBomb();
                    break;
            }
        }

        private void DetonateBomb()
        {
            if (Enemy.EnemySpawner.Instance != null) Enemy.EnemySpawner.Instance.ClearAllEnemies();
            if (Effects.CameraShake.Instance != null) Effects.CameraShake.Instance.Shake(0.5f, 0.5f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Explosion");
        }

        private void ReturnToPool()
        {
            if (ObjectPool.Instance != null && ObjectPool.Instance.HasPool(poolTag))
            {
                ObjectPool.Instance.ReturnObject(poolTag, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
