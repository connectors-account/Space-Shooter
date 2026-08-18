using System.Collections;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Base class for pickups. Drifts downward and, on contact with the player,
    /// applies its effect (optionally for a limited duration).
    /// </summary>
    public abstract class PowerUpBase : MonoBehaviour
    {
        [Tooltip("Effect duration in seconds. 0 = instant / permanent.")]
        public float duration = 0f;

        [Tooltip("Downward drift speed in world units per second.")]
        public float fallSpeed = 2f;

        private bool _collected;

        protected virtual void Update()
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;

            var player = other.GetComponent<PlayerController>();
            if (player == null) player = other.GetComponentInParent<PlayerController>();
            if (player == null) return;

            _collected = true;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.powerUpSFX);

            // Hide visuals immediately but keep the object alive for the timer.
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            ApplyWithTimer(player.gameObject);
        }

        /// <summary>
        /// Applies the effect and, if <see cref="duration"/> &gt; 0, schedules expiry.
        /// Instant power-ups destroy their pickup object right away.
        /// </summary>
        public void ApplyWithTimer(GameObject player)
        {
            Apply(player);

            if (duration > 0f)
            {
                if (isActiveAndEnabled) StartCoroutine(ExpireRoutine(player));
            }
            else if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator ExpireRoutine(GameObject player)
        {
            yield return new WaitForSeconds(duration);
            Expire(player);
            if (Application.isPlaying) Destroy(gameObject);
        }

        /// <summary>Applies the power-up effect to the player.</summary>
        public abstract void Apply(GameObject player);

        /// <summary>Reverses the power-up effect when its duration ends.</summary>
        public abstract void Expire(GameObject player);
    }
}
