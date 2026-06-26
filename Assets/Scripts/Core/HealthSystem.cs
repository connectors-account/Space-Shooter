using System;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// A small, self-contained, reusable health container that can be attached to any GameObject
    /// (player, enemy, destructible prop) or used as a plain field inside another component.
    /// <para>
    /// The game's <see cref="SpaceShooter.Player.PlayerController"/> and
    /// <see cref="SpaceShooter.Enemies.Enemy"/> manage their own health inline for tight control
    /// over respawn / phase logic, but this component exposes the exact same semantics in a single
    /// drop-in place and is what the <c>Player.prefab</c> / <c>Enemy.prefab</c> reference for their
    /// "X hits = death" rule. It can also be used standalone for new objects.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class HealthSystem : MonoBehaviour
    {
        [Tooltip("Maximum (and starting) health value.")]
        [SerializeField] private int _maxHealth = 100;

        [Tooltip("If > 0, the object dies after this many hits regardless of per-hit damage. " +
                 "Used by the player's '3 hits = game over' rule. Set to 0 to use raw health only.")]
        [SerializeField] private int _maxHits = 0;

        private int _currentHealth;
        private int _hitsTaken;
        private bool _isDead;

        /// <summary>Raised whenever health changes. Args: (current, max).</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised once when health (or the hit counter) reaches its limit.</summary>
        public event Action Died;

        /// <summary>Raised when the object is damaged but survives. Arg: damage amount.</summary>
        public event Action<int> Damaged;

        /// <summary>Raised when the object is healed. Arg: heal amount.</summary>
        public event Action<int> Healed;

        /// <summary>Current health value (never below zero).</summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>Maximum health value.</summary>
        public int MaxHealth => _maxHealth;

        /// <summary>Number of hits taken so far (relevant when <see cref="_maxHits"/> &gt; 0).</summary>
        public int HitsTaken => _hitsTaken;

        /// <summary>True once this object has died.</summary>
        public bool IsDead => _isDead;

        /// <summary>Health as a 0..1 fraction, convenient for driving a health bar fill.</summary>
        public float Normalized => _maxHealth > 0 ? Mathf.Clamp01((float)_currentHealth / _maxHealth) : 0f;

        private void Awake()
        {
            // Ensure sensible state even if Configure() is never called.
            if (_currentHealth == 0 && !_isDead)
            {
                ResetHealth();
            }
        }

        /// <summary>
        /// Configures the health container at runtime (e.g. when a pooled object is spawned).
        /// </summary>
        /// <param name="maxHealth">Maximum/starting health.</param>
        /// <param name="maxHits">Optional hit cap (0 to disable).</param>
        public void Configure(int maxHealth, int maxHits = 0)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _maxHits = Mathf.Max(0, maxHits);
            ResetHealth();
        }

        /// <summary>Restores the object to full health and clears the dead / hit state.</summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _hitsTaken = 0;
            _isDead = false;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>
        /// Applies damage. Returns true if this damage was fatal.
        /// </summary>
        /// <param name="amount">Positive damage amount.</param>
        public bool TakeDamage(int amount)
        {
            if (_isDead || amount <= 0)
            {
                return false;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _hitsTaken++;
            Damaged?.Invoke(amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            bool hitsExhausted = _maxHits > 0 && _hitsTaken >= _maxHits;
            if (_currentHealth <= 0 || hitsExhausted)
            {
                _isDead = true;
                Died?.Invoke();
                return true;
            }

            return false;
        }

        /// <summary>Adds health, clamped to <see cref="MaxHealth"/>. No effect if already dead.</summary>
        /// <param name="amount">Positive heal amount.</param>
        public void Heal(int amount)
        {
            if (_isDead || amount <= 0)
            {
                return;
            }

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            Healed?.Invoke(amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        /// <summary>Immediately sets health to zero and raises <see cref="Died"/> once.</summary>
        public void Kill()
        {
            if (_isDead)
            {
                return;
            }

            _currentHealth = 0;
            _isDead = true;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            Died?.Invoke();
        }
    }
}
