using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Moves straight down, then dives diagonally toward the player's X once it
    /// reaches the player's Y level. Fires an aimed bullet on a timer.
    /// </summary>
    public class EnemyDiver : EnemyBase
    {
        #region Fields
        private bool _diving;
        private float _targetX;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            _maxHealth = GameConstants.ENEMY_DIVER_HEALTH;
            _scoreValue = GameConstants.ENEMY_DIVER_SCORE;
            _moveSpeed = GameConstants.ENEMY_DIVER_SPEED;

            if (_shooter == null) _shooter = gameObject.AddComponent<EnemyShooter>();
            _shooter.Configure(ShootingPattern.Aimed, GameConstants.ENEMY_DIVER_FIRE_RATE);
        }
        #endregion

        #region Movement
        protected override void Move()
        {
            float playerY = GameConstants.PLAYER_START_Y;
            Vector3 playerPos = GetPlayerPos();
            playerY = playerPos.y;

            if (!_diving)
            {
                transform.Translate(Vector3.down * _moveSpeed * Time.deltaTime, Space.World);
                if (transform.position.y <= playerY + 2f)
                {
                    _diving = true;
                    _targetX = playerPos.x;
                }
            }
            else
            {
                Vector3 target = new Vector3(_targetX, GameConstants.CAMERA_BOTTOM - 1f, 0f);
                float diveSpeed = _moveSpeed * GameConstants.ENEMY_DIVER_DIVE_MULTIPLIER;
                transform.position = Vector3.MoveTowards(transform.position, target, diveSpeed * Time.deltaTime);
            }
        }

        private Vector3 GetPlayerPos()
        {
            if (GameManager.Instance != null)
            {
                GameObject p = GameManager.Instance.GetPlayer();
                if (p != null) return p.transform.position;
            }
            return new Vector3(transform.position.x, GameConstants.PLAYER_START_Y, 0f);
        }
        #endregion
    }
}
