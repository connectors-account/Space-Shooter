using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Player
{
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        TripleShot,
        SpeedBoost
    }

    /// <summary>
    /// Manages active timed power-ups on the player. Each lasts 10 seconds and drives a
    /// visual glow indicator. Exposes remaining time for the HUD.
    /// </summary>
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerPowerUp : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float duration = 10f;
        [SerializeField] private SpriteRenderer shipSprite;
        [SerializeField] private float speedBoostMultiplier = 1.6f;
        [SerializeField] private float rapidFireMultiplier = 0.4f;

        private PlayerController _controller;
        private PlayerShooter _shooter;
        private PlayerHealth _health;

        private readonly System.Collections.Generic.Dictionary<PowerUpType, Coroutine> _active =
            new System.Collections.Generic.Dictionary<PowerUpType, Coroutine>();

        private Color _baseColor;

        public event Action<PowerUpType> OnPowerUpActivated;
        public event Action<PowerUpType> OnPowerUpExpired;
        public event Action<PowerUpType, float, float> OnPowerUpTick; // type, remaining, total

        public bool HasActivePowerUp => _active.Count > 0;

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _shooter = GetComponent<PlayerShooter>();
            _health = GetComponent<PlayerHealth>();
            if (shipSprite == null)
            {
                shipSprite = GetComponent<SpriteRenderer>();
            }
            if (shipSprite != null)
            {
                _baseColor = shipSprite.color;
            }
        }

        /// <summary>Activates (or refreshes) a power-up of the given type.</summary>
        public void Activate(PowerUpType type)
        {
            if (_active.TryGetValue(type, out Coroutine existing) && existing != null)
            {
                StopCoroutine(existing);
            }

            AudioManager.Instance?.PlaySFX("powerup");

            switch (type)
            {
                case PowerUpType.Shield:
                    _health?.ActivateShield();
                    break;
                case PowerUpType.RapidFire:
                    _shooter?.SetMode(ShootMode.Rapid);
                    _shooter?.SetFireRateMultiplier(rapidFireMultiplier);
                    break;
                case PowerUpType.TripleShot:
                    _shooter?.SetMode(ShootMode.Triple);
                    break;
                case PowerUpType.SpeedBoost:
                    _controller?.SetSpeedMultiplier(speedBoostMultiplier);
                    break;
            }

            OnPowerUpActivated?.Invoke(type);
            _active[type] = StartCoroutine(PowerUpTimer(type));
            UpdateGlow();
        }

        private IEnumerator PowerUpTimer(PowerUpType type)
        {
            float remaining = duration;
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                OnPowerUpTick?.Invoke(type, Mathf.Max(0f, remaining), duration);
                yield return null;
            }

            Deactivate(type);
        }

        private void Deactivate(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.RapidFire:
                    _shooter?.ResetMode();
                    _shooter?.SetFireRateMultiplier(1f);
                    break;
                case PowerUpType.TripleShot:
                    _shooter?.ResetMode();
                    break;
                case PowerUpType.SpeedBoost:
                    _controller?.SetSpeedMultiplier(1f);
                    break;
                case PowerUpType.Shield:
                    // Shield ends when absorbed or timer runs out; nothing extra to reset.
                    break;
            }

            _active.Remove(type);
            OnPowerUpExpired?.Invoke(type);
            UpdateGlow();
        }

        private void UpdateGlow()
        {
            if (shipSprite == null) return;

            if (_active.Count == 0)
            {
                shipSprite.color = _baseColor;
                return;
            }

            // Blend a glow tint based on the most recently active power-up set.
            Color glow = _baseColor;
            foreach (var kvp in _active)
            {
                glow = Color.Lerp(glow, GlowColor(kvp.Key), 0.5f);
            }
            shipSprite.color = glow;
        }

        private static Color GlowColor(PowerUpType type)
        {
            switch (type)
            {
                case PowerUpType.Shield: return new Color(0.3f, 0.6f, 1f);
                case PowerUpType.RapidFire: return new Color(1f, 0.5f, 0.2f);
                case PowerUpType.TripleShot: return new Color(0.4f, 1f, 0.4f);
                case PowerUpType.SpeedBoost: return new Color(1f, 1f, 0.3f);
                default: return Color.white;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _active.Clear();
            if (shipSprite != null)
            {
                shipSprite.color = _baseColor;
            }
        }
    }
}
