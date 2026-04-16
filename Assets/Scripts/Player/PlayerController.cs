using SpaceShooter.Combat;
using SpaceShooter.Core;
using UnityEngine;

namespace SpaceShooter.Player
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(PlayerWeapon))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private Vector2 minBounds = new Vector2(-8f, -4.5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(8f, 4.5f);
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip deathSfx;

        private Health health;
        private PlayerWeapon weapon;

        private void Awake()
        {
            health = GetComponent<Health>();
            weapon = GetComponent<PlayerWeapon>();
        }

        private void OnEnable()
        {
            health.OnDied += OnDeath;
        }

        private void OnDisable()
        {
            health.OnDied -= OnDeath;
        }

        private void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            HandleMovement();
            HandleFireInput();
        }

        private void HandleMovement()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 movement = new Vector3(horizontal, vertical, 0f).normalized;

            transform.position += movement * (moveSpeed * Time.deltaTime);

            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minBounds.x, maxBounds.x);
            clampedPosition.y = Mathf.Clamp(clampedPosition.y, minBounds.y, maxBounds.y);
            transform.position = clampedPosition;
        }

        private void HandleFireInput()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                weapon.TryFire();
            }
        }

        private void OnDeath(Health deadHealth)
        {
            if (audioSource != null && deathSfx != null)
            {
                audioSource.PlayOneShot(deathSfx);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
    }
}
