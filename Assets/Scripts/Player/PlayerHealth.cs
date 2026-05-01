using SpaceShooter.Core;
using SpaceShooter.Visual;
using UnityEngine;

namespace SpaceShooter.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        private GameManager _gameManager;
        private int _currentHealth;
        private float _shieldTimer;

        public int MaxHealth { get; private set; }
        public int CurrentHealth => _currentHealth;
        public bool HasShield => _shieldTimer > 0f;

        public Faction Faction => Faction.Player;

        public void Initialize(GameManager gameManager, int maxHealth)
        {
            _gameManager = gameManager;
            MaxHealth = maxHealth;
            _currentHealth = maxHealth;
            _shieldTimer = 0f;
            _gameManager.UiManager.RefreshHealth(_currentHealth, MaxHealth, HasShield);
        }

        private void Update()
        {
            if (_shieldTimer <= 0f) return;
            _shieldTimer -= Time.deltaTime;
            if (_shieldTimer <= 0f)
            {
                _gameManager.UiManager.RefreshHealth(_currentHealth, MaxHealth, false);
            }
        }

        public void ApplyDamage(int amount, Vector3 hitPosition)
        {
            if (_gameManager.CurrentState != GameState.Playing) return;

            if (HasShield)
            {
                _shieldTimer = 0f;
                _gameManager.UiManager.RefreshHealth(_currentHealth, MaxHealth, false);
                Sound.SoundManager.Instance?.PlaySfx("shield_break");
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            EffectManager.Instance?.SpawnHit(hitPosition);
            Sound.SoundManager.Instance?.PlaySfx("player_hit");
            _gameManager.UiManager.RefreshHealth(_currentHealth, MaxHealth, HasShield);

            if (_currentHealth <= 0)
            {
                _gameManager.GameOver();
            }
        }

        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(MaxHealth, _currentHealth + amount);
            _gameManager.UiManager.RefreshHealth(_currentHealth, MaxHealth, HasShield);
            Sound.SoundManager.Instance?.PlaySfx("powerup");
        }

        public void ActivateShield(float duration)
        {
            _shieldTimer = Mathf.Max(_shieldTimer, duration);
            _gameManager.UiManager.RefreshHealth(_currentHealth, MaxHealth, true);
            Sound.SoundManager.Instance?.PlaySfx("powerup");
        }
    }
}
