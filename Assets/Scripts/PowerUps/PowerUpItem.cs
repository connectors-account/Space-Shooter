using UnityEngine;

namespace SpaceShooter.PowerUps
{
    public enum PowerUpType
    {
        HealthRestore,
        RapidFire,
        Shield
    }

    /// <summary>
    /// Collectable power-up that drifts downward. On contact with the player
    /// it applies its effect and deactivates.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PowerUpItem : MonoBehaviour
    {
        [SerializeField] private PowerUpType type;
        [SerializeField] private float driftSpeed = 2f;
        [SerializeField] private float lifetime = 8f;
        [SerializeField] private float bobAmplitude = 0.3f;
        [SerializeField] private float bobFrequency = 2f;

        [Header("Effect Values")]
        [SerializeField] private int healthAmount = 1;
        [SerializeField] private float rapidFireDuration = 5f;
        [SerializeField] private float shieldDuration = 6f;

        private float timer;
        private float startY;
        private float timeAlive;

        public PowerUpType Type => type;

        private void OnEnable()
        {
            timer = lifetime;
            startY = transform.position.y;
            timeAlive = 0f;
        }

        private void Update()
        {
            // Drift downward with a small bob
            timeAlive += Time.deltaTime;
            Vector3 pos = transform.position;
            pos.y -= driftSpeed * Time.deltaTime;
            pos.x += Mathf.Sin(timeAlive * bobFrequency * Mathf.PI) * bobAmplitude * Time.deltaTime;
            transform.position = pos;

            // Rotate slowly for visual flair
            transform.Rotate(0f, 0f, 90f * Time.deltaTime);

            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                gameObject.SetActive(false);
            }

            // Off-screen check
            if (Camera.main != null)
            {
                Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
                if (vp.y < -0.1f)
                    gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Apply this power-up's effect to the player.
        /// Called by PlayerController.OnTriggerEnter2D.
        /// </summary>
        public void ApplyEffect(Player.PlayerController player)
        {
            switch (type)
            {
                case PowerUpType.HealthRestore:
                    player.RestoreHealth(healthAmount);
                    break;
                case PowerUpType.RapidFire:
                    player.ActivateRapidFire(rapidFireDuration);
                    break;
                case PowerUpType.Shield:
                    player.ActivateShield(shieldDuration);
                    break;
            }

            gameObject.SetActive(false);
        }

        public void SetType(PowerUpType newType)
        {
            type = newType;

            // Set colour based on type
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                switch (type)
                {
                    case PowerUpType.HealthRestore:
                        sr.color = Color.green;
                        break;
                    case PowerUpType.RapidFire:
                        sr.color = Color.yellow;
                        break;
                    case PowerUpType.Shield:
                        sr.color = Color.cyan;
                        break;
                }
            }
        }
    }
}
