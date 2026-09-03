using UnityEngine;
using SpaceShooter.Core;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Orbits a random center point using sin/cos while slowly drifting down.
    /// Fires a full circle of bullets on a timer.
    /// </summary>
    public class EnemyCircler : EnemyBase
    {
        #region Fields
        private Vector3 _orbitCenter;
        private float _orbitAngle;
        private float _driftDown;
        #endregion

        #region Setup
        protected override void Awake()
        {
            base.Awake();
            _maxHealth = GameConstants.ENEMY_CIRCLER_HEALTH;
            _scoreValue = GameConstants.ENEMY_CIRCLER_SCORE;
            _moveSpeed = GameConstants.ENEMY_CIRCLER_SPEED;

            if (_shooter == null) _shooter = gameObject.AddComponent<EnemyShooter>();
            _shooter.Configure(ShootingPattern.Circle, GameConstants.ENEMY_CIRCLER_FIRE_RATE);
        }

        protected override void Start()
        {
            base.Start();
            float cx = Random.Range(GameConstants.CAMERA_LEFT + 2f, GameConstants.CAMERA_RIGHT - 2f);
            float cy = Random.Range(1.5f, GameConstants.CAMERA_TOP - 1f);
            _orbitCenter = new Vector3(cx, cy, 0f);
            _orbitAngle = Random.Range(0f, Mathf.PI * 2f);
        }
        #endregion

        #region Movement
        protected override void Move()
        {
            _orbitAngle += GameConstants.ENEMY_CIRCLER_ORBIT_SPEED * Time.deltaTime;
            _driftDown += _moveSpeed * 0.15f * Time.deltaTime;

            float r = GameConstants.ENEMY_CIRCLER_ORBIT_RADIUS;
            float x = _orbitCenter.x + Mathf.Cos(_orbitAngle) * r;
            float y = _orbitCenter.y + Mathf.Sin(_orbitAngle) * r - _driftDown;
            transform.position = new Vector3(x, y, 0f);
        }
        #endregion
    }
}
