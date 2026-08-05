using System;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Audio;
using SpaceShooter.Utilities;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Player health / lives / shield. Emits events consumed by the HUD.
    /// Handles a temporary shield that absorbs one hit's worth of damage while
    /// active, invincibility frames after a hit, and camera shake on damage.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Lives")]
        [SerializeField] private int startingLives = 3;
        [SerializeField] private int maxLives = 5;

        [Header("Shield")]
        [SerializeField] private float defaultShieldDuration = 6f;

        [Header("Camera shake on hit")]
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeMagnitude = 0.35f;

        /// <summary>Fired when damage is taken (remaining lives).</summary>
        public event Action<int> OnDamaged;
        /// <summary>Fired when lives change for any reason (current lives).</summary>
        public event Action<int> OnLivesChanged;
        /// <summary>Fired when the player dies (no lives remaining).</summary>
        public event Action OnDied;
        public event Action<float> OnShieldActivated;   // duration
        public event Action OnShieldDeactivated;

        public int Lives { get; private set; }
        public int MaxLives => maxLives;
        public bool ShieldActive { get; private set; }

        private float _shieldTimer;
        private PlayerController _controller;
        private GameObject _shieldVisual;
        private SpriteRenderer _shieldRenderer;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            Lives = startingLives;
            BuildShieldVisual();
        }

        private void OnEnable()
        {
            Lives = startingLives;
        }

        private void Start()
        {
            OnLivesChanged?.Invoke(Lives);
        }

        private void BuildShieldVisual()
        {
            _shieldVisual = new GameObject("ShieldVisual");
            _shieldVisual.transform.SetParent(transform, false);
            _shieldRenderer = _shieldVisual.AddComponent<SpriteRenderer>();
            _shieldRenderer.sprite = SpriteGenerator.CreateShieldSprite();
            _shieldRenderer.sortingOrder = 5;
            _shieldVisual.SetActive(false);
        }

        private void Update()
        {
            if (ShieldActive)
            {
                _shieldTimer -= Time.deltaTime;
                if (_shieldTimer <= 0f)
                    DeactivateShield();
            }
        }

        /// <summary>Apply one unit of damage. Ignored while invincible.</summary>
        public void TakeDamage()
        {
            if (_controller != null && _controller.IsInvincible)
                return;

            if (ShieldActive)
            {
                // Shield absorbs this hit and drops.
                DeactivateShield();
                _controller.BeginInvincibility(0.75f);
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(Constants.SfxShieldDown);
                CameraShake.ShakeStatic(shakeDuration * 0.5f, shakeMagnitude * 0.5f);
                return;
            }

            Lives = Mathf.Max(0, Lives - 1);
            OnDamaged?.Invoke(Lives);
            OnLivesChanged?.Invoke(Lives);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(Constants.SfxPlayerHit);
            CameraShake.ShakeStatic(shakeDuration, shakeMagnitude);

            if (Lives <= 0)
            {
                OnDied?.Invoke();
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }
            else
            {
                _controller.BeginInvincibility();
            }
        }

        public void AddLife()
        {
            Lives = Mathf.Min(maxLives, Lives + 1);
            OnLivesChanged?.Invoke(Lives);
        }

        public void ActivateShield()
        {
            ActivateShield(defaultShieldDuration);
        }

        public void ActivateShield(float duration)
        {
            ShieldActive = true;
            _shieldTimer = duration;
            if (_shieldVisual != null) _shieldVisual.SetActive(true);
            OnShieldActivated?.Invoke(duration);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(Constants.SfxShieldUp);
        }

        public void DeactivateShield()
        {
            if (!ShieldActive) return;
            ShieldActive = false;
            _shieldTimer = 0f;
            if (_shieldVisual != null) _shieldVisual.SetActive(false);
            OnShieldDeactivated?.Invoke();
        }

        public void ResetHealth()
        {
            Lives = startingLives;
            DeactivateShield();
            OnLivesChanged?.Invoke(Lives);
        }
    }
}
