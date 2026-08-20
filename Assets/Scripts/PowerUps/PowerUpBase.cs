using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Utilities;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Abstract power-up pickup. Drifts downward, rotates slowly, and is collected on trigger
    /// with the player. Timed power-ups start a duration timer once applied and auto-remove.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class PowerUpBase : MonoBehaviour
    {
        [Header("Movement")]
        public float fallSpeed = 2f;
        public float rotationSpeed = 90f;

        [Header("Duration")]
        [Tooltip("Seconds the effect lasts. Instant/one-shot power-ups can ignore this.")]
        public float duration = 10f;

        protected PlayerShooter Shooter;
        protected PlayerHealth Health;
        private bool _collected;
        private float _remaining;
        private bool _active;

        protected virtual void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col == null)
            {
                var c = gameObject.AddComponent<CircleCollider2D>();
                c.isTrigger = true;
            }
            else col.isTrigger = true;
            gameObject.tag = "PowerUp";

            var sr = GetComponent<SpriteRenderer>();
            if (sr != null && sr.sprite == null) sr.sprite = CreateSprite();
        }

        /// <summary>Runtime icon sprite for the pickup. Override per power-up for a distinct color.</summary>
        protected virtual Sprite CreateSprite()
        {
            return Utilities.SpriteGenerator.CreateCircle(14, Color.white);
        }

        protected virtual void Update()
        {
            if (!_collected)
            {
                // Drift + spin while floating in the world.
                transform.position += Vector3.down * fallSpeed * Time.deltaTime;
                transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

                if (ScreenBounds.Instance != null &&
                    transform.position.y < ScreenBounds.Instance.Bottom - 1f)
                {
                    Destroy(gameObject);
                }
            }
            else if (_active)
            {
                _remaining -= Time.deltaTime;
                if (_remaining <= 0f)
                {
                    Remove();
                    _active = false;
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected || !other.CompareTag("Player")) return;

            Shooter = other.GetComponent<PlayerShooter>();
            Health = other.GetComponent<PlayerHealth>();
            if (Shooter == null && Health == null) return;

            _collected = true;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("powerup");

            Apply(Shooter, Health);

            // Hide the pickup visual while the timed effect runs.
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            if (IsTimed)
            {
                _remaining = duration;
                _active = true;
                var hud = FindObjectOfType<UI.HUDController>();
                if (hud != null) hud.RegisterActivePowerUp(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>Override to false for instant one-shot pickups (e.g. shield hit).</summary>
        protected virtual bool IsTimed => true;

        /// <summary>Remaining seconds of the active effect (0..duration), for HUD timer bars.</summary>
        public float RemainingNormalized => IsTimed && _active ? Mathf.Clamp01(_remaining / duration) : 0f;

        public abstract void Apply(PlayerShooter shooter, PlayerHealth health);
        public abstract void Remove();
    }
}
