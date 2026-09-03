using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Flies to an assigned formation slot, then oscillates left/right sinusoidally.
    /// Fires a straight bullet downward on a timer.
    /// </summary>
    public class EnemyFormation : EnemyBase
    {
        #region Fields
        private Vector3 _formationPosition;
        private bool _inPosition;
        private float _phaseOffset;
        private float _baseX;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            _maxHealth = GameConstants.ENEMY_FORMATION_HEALTH;
            _scoreValue = GameConstants.ENEMY_FORMATION_SCORE;
            _moveSpeed = GameConstants.ENEMY_FORMATION_SPEED;

            if (_shooter == null) _shooter = gameObject.AddComponent<EnemyShooter>();
            _shooter.Configure(ShootingPattern.Straight, GameConstants.ENEMY_FORMATION_FIRE_RATE);
        }

        protected override void Start()
        {
            base.Start();
            // Compute a target formation row position near the top of the screen.
            float rowY = GameConstants.CAMERA_TOP - 1.5f;
            float x = Mathf.Clamp(transform.position.x, GameConstants.CAMERA_LEFT + 1f, GameConstants.CAMERA_RIGHT - 1f);
            _formationPosition = new Vector3(x, rowY, 0f);
            _baseX = x;
            _phaseOffset = _formationIndex * 0.6f;
        }
        #endregion

        #region Movement
        protected override void Move()
        {
            if (!_inPosition)
            {
                transform.position = Vector3.MoveTowards(transform.position, _formationPosition, _moveSpeed * Time.deltaTime);
                if ((transform.position - _formationPosition).sqrMagnitude < 0.01f)
                    _inPosition = true;
            }
            else
            {
                float x = _baseX + Mathf.Sin(Time.time * GameConstants.ENEMY_FORMATION_OSCILLATE_FREQ + _phaseOffset)
                          * GameConstants.ENEMY_FORMATION_OSCILLATE_AMP;
                x = Mathf.Clamp(x, GameConstants.CAMERA_LEFT + 0.5f, GameConstants.CAMERA_RIGHT - 0.5f);
                transform.position = new Vector3(x, _formationPosition.y, 0f);
            }
        }
        #endregion
    }
}
