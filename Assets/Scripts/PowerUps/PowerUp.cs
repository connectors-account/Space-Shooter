using SpaceShooter.Audio;
using SpaceShooter.Core;
using SpaceShooter.Player;
using SpaceShooter.Utils;
using UnityEngine;

namespace SpaceShooter.PowerUps
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        HealthRestore
    }

    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class PowerUp : MonoBehaviour
    {
        private PowerUpType _type;
        private float _fallSpeed = 2f;

        public static void SpawnRandom(Vector3 position)
        {
            if (Random.value > 0.25f)
            {
                return;
            }

            var powerupObject = new GameObject("PowerUp");
            powerupObject.transform.position = position;
            var powerup = powerupObject.AddComponent<PowerUp>();

            var random = Random.Range(0, 3);
            powerup.Initialize((PowerUpType)random);
        }

        private void Initialize(PowerUpType type)
        {
            _type = type;
            name = type + "PowerUp";

            var color = type switch
            {
                PowerUpType.Shield => new Color(0.3f, 0.95f, 1f),
                PowerUpType.RapidFire => new Color(1f, 0.95f, 0.2f),
                PowerUpType.HealthRestore => new Color(0.3f, 1f, 0.35f),
                _ => Color.white
            };

            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.GetSprite(color, ShapeType.Circle, 24);

            var collider = GetComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.35f;

            var rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
            rb.gravityScale = 0f;

            Destroy(gameObject, 8f);
        }

        private void Update()
        {
            transform.position += Vector3.down * (_fallSpeed * Time.deltaTime);

            if (transform.position.y < -6f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                return;
            }

            ApplyTo(player);
            AudioManager.Instance?.PlayPowerUp();
            Destroy(gameObject);
        }

        private void ApplyTo(PlayerController player)
        {
            switch (_type)
            {
                case PowerUpType.Shield:
                    player.ActivateShield(5f);
                    break;
                case PowerUpType.RapidFire:
                    player.ActivateRapidFire(6f);
                    break;
                case PowerUpType.HealthRestore:
                    GameManager.Instance?.HealPlayer(1);
                    break;
            }
        }
    }
}
