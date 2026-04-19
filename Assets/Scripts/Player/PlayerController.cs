using UnityEngine;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player movement + fire control using keyboard input and timing modifiers.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private Vector2 playAreaMin = new Vector2(-8.5f, -4.3f);
        [SerializeField] private Vector2 playAreaMax = new Vector2(8.5f, 4.3f);

        [Header("Shooting")]
        [SerializeField] private Combat.Bullet bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float baseFireCooldown = 0.22f;

        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;

        private float nextShootTime;
        private float rapidFireEndTime;
        private float rapidFireMultiplier = 1f;

        public bool IsRapidFireActive => Time.time < rapidFireEndTime;

        public event System.Action OnPlayerDied;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }
        }

        private void Start()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandleDeath;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            var input = Input.InputHandler.Instance;
            var gameManager = Core.GameManager.Instance;

            if (input == null || gameManager == null)
            {
                return;
            }

            if (input.PausePressedThisFrame)
            {
                gameManager.TogglePause();
            }

            if (!gameManager.IsGameplayActive() || (playerHealth != null && !playerHealth.IsAlive))
            {
                return;
            }

            HandleMovement(input.MoveInput);

            if (input.ShootHeld)
            {
                HandleShooting();
            }
        }

        public void ResetPlayer()
        {
            transform.position = new Vector3(0f, -3.5f, 0f);
            nextShootTime = 0f;
            rapidFireEndTime = 0f;
            rapidFireMultiplier = 1f;
            playerHealth?.ResetHealth();
            gameObject.SetActive(true);
        }

        public void ActivateRapidFire(float duration, float multiplier)
        {
            rapidFireMultiplier = Mathf.Max(1f, multiplier);
            rapidFireEndTime = Mathf.Max(rapidFireEndTime, Time.time + duration);
        }

        private void HandleMovement(Vector2 moveInput)
        {
            Vector3 delta = (Vector3)(moveInput * moveSpeed * Time.deltaTime);
            Vector3 nextPos = transform.position + delta;
            nextPos.x = Mathf.Clamp(nextPos.x, playAreaMin.x, playAreaMax.x);
            nextPos.y = Mathf.Clamp(nextPos.y, playAreaMin.y, playAreaMax.y);
            transform.position = nextPos;
        }

        private void HandleShooting()
        {
            float cooldown = baseFireCooldown;
            if (IsRapidFireActive)
            {
                cooldown /= rapidFireMultiplier;
            }

            if (Time.time < nextShootTime || bulletPrefab == null || firePoint == null)
            {
                return;
            }

            Combat.Bullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.Initialize(Vector2.up, Combat.Faction.Player);
            Audio.SoundManager.Instance?.PlayShoot();

            nextShootTime = Time.time + cooldown;
        }

        private void HandleDeath()
        {
            OnPlayerDied?.Invoke();
            gameObject.SetActive(false);
        }
    }
}
