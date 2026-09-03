using System;
using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Pickups;

namespace SpaceShooter.Player
{
    /// <summary>
    /// Applies collected power-ups to the player, managing timed effects
    /// (weapon patterns, speed boost) and reverting them when they expire.
    /// </summary>
    [RequireComponent(typeof(PlayerShooter))]
    public class PlayerPowerUp : MonoBehaviour
    {
        #region Events
        /// <summary>Fired when a timed power-up starts. Args: type, duration.</summary>
        public static event Action<PowerUpType, float> OnPowerUpActivated;
        /// <summary>Fired when the active timed power-up expires.</summary>
        public static event Action OnPowerUpExpired;
        #endregion

        #region Fields
        private PlayerShooter _shooter;
        private PlayerHealth _health;
        private PlayerController _controller;

        private Coroutine _weaponTimer;
        private Coroutine _speedTimer;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            _shooter = GetComponent<PlayerShooter>();
            _health = GetComponent<PlayerHealth>();
            _controller = GetComponent<PlayerController>();
        }
        #endregion

        #region Apply
        /// <summary>Applies a power-up effect based on its type.</summary>
        public void ApplyPowerUp(PowerUpType t)
        {
            switch (t)
            {
                case PowerUpType.Shield:
                    if (_health != null) _health.AddShield();
                    break;

                case PowerUpType.HealthPack:
                    if (_health != null) _health.Heal(GameConstants.POWERUP_HEALTHPACK_AMOUNT);
                    break;

                case PowerUpType.TripleShot:
                    ActivateTimedWeapon(ShootPattern.Triple, PowerUpType.TripleShot);
                    break;

                case PowerUpType.Spread5:
                    ActivateTimedWeapon(ShootPattern.Spread5, PowerUpType.Spread5);
                    break;

                case PowerUpType.Laser:
                    ActivateTimedWeapon(ShootPattern.Laser, PowerUpType.Laser);
                    break;

                case PowerUpType.SpeedBoost:
                    ActivateSpeedBoost();
                    break;

                case PowerUpType.Nuke:
                    // Handled by PowerUp/WaveManager directly; nothing to apply here.
                    break;
            }
        }
        #endregion

        #region Timed Weapon
        private void ActivateTimedWeapon(ShootPattern pattern, PowerUpType type)
        {
            if (_shooter == null) return;
            _shooter.SetPattern(pattern);

            if (_weaponTimer != null) StopCoroutine(_weaponTimer);
            _weaponTimer = StartCoroutine(WeaponTimerRoutine(type));
        }

        private IEnumerator WeaponTimerRoutine(PowerUpType type)
        {
            OnPowerUpActivated?.Invoke(type, GameConstants.POWERUP_TIMED_DURATION);
            yield return new WaitForSeconds(GameConstants.POWERUP_TIMED_DURATION);
            if (_shooter != null) _shooter.ResetToDefault();
            OnPowerUpExpired?.Invoke();
            _weaponTimer = null;
        }
        #endregion

        #region Speed Boost
        private void ActivateSpeedBoost()
        {
            if (_controller == null) return;
            _controller.SetMoveSpeed(GameConstants.PLAYER_BOOSTED_MOVE_SPEED);

            if (_speedTimer != null) StopCoroutine(_speedTimer);
            _speedTimer = StartCoroutine(SpeedTimerRoutine());
        }

        private IEnumerator SpeedTimerRoutine()
        {
            OnPowerUpActivated?.Invoke(PowerUpType.SpeedBoost, GameConstants.PLAYER_SPEED_BOOST_DURATION);
            yield return new WaitForSeconds(GameConstants.PLAYER_SPEED_BOOST_DURATION);
            if (_controller != null) _controller.SetMoveSpeed(GameConstants.PLAYER_MOVE_SPEED);
            OnPowerUpExpired?.Invoke();
            _speedTimer = null;
        }
        #endregion
    }
}
