using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Bullets;

namespace SpaceShooter.Enemy
{
    /// <summary>Firing behaviours an enemy can use.</summary>
    public enum ShootingPattern { None, Straight, Aimed, Circle, Spread, Spiral }

    /// <summary>
    /// Component attached to enemies that fires bullets in a configured pattern
    /// at a fixed rate. Uses BulletPattern static helpers; targets the player for Aimed.
    /// </summary>
    public class EnemyShooter : MonoBehaviour
    {
        #region Fields
        [SerializeField] private ShootingPattern _pattern = ShootingPattern.Straight;
        [SerializeField] private float _fireRate = 1.5f;

        [Header("Pattern Params")]
        [SerializeField] private int _circleCount = 8;
        [SerializeField] private int _spreadCount = 5;
        [SerializeField] private float _spreadArc = 60f;
        [SerializeField] private int _spiralArms = 3;
        [SerializeField] private float _spiralRotSpeed = 120f;

        private Coroutine _shootRoutine;
        private Coroutine _spiralRoutine;
        private bool _shooting;
        #endregion

        #region Properties
        public ShootingPattern Pattern { get => _pattern; set => _pattern = value; }
        public float FireRate { get => _fireRate; set => _fireRate = value; }
        #endregion

        #region Control
        /// <summary>Starts the firing loop for the current pattern.</summary>
        public void StartShooting()
        {
            if (_shooting) return;
            _shooting = true;

            if (_pattern == ShootingPattern.Spiral)
            {
                _spiralRoutine = StartCoroutine(
                    BulletPattern.Spiral(this, () => transform.position, _spiralArms, _spiralRotSpeed, true, GameConstants.BOSS_SPIRAL_FIRE_RATE));
            }
            else if (_pattern != ShootingPattern.None)
            {
                _shootRoutine = StartCoroutine(ShootLoop());
            }
        }

        /// <summary>Stops all firing.</summary>
        public void StopShooting()
        {
            _shooting = false;
            if (_shootRoutine != null) { StopCoroutine(_shootRoutine); _shootRoutine = null; }
            if (_spiralRoutine != null) { StopCoroutine(_spiralRoutine); _spiralRoutine = null; }
        }

        private void OnDisable()
        {
            StopShooting();
        }
        #endregion

        #region Loop
        private IEnumerator ShootLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.05f, _fireRate));
            // Small initial delay so freshly spawned enemies do not all fire at once.
            yield return new WaitForSeconds(Random.Range(0f, _fireRate));

            while (_shooting)
            {
                FireOnce();
                yield return wait;
            }
        }

        /// <summary>Fires a single volley of the current pattern immediately.</summary>
        public void FireOnce()
        {
            Vector3 origin = transform.position;
            switch (_pattern)
            {
                case ShootingPattern.Straight:
                    BulletPattern.Straight(origin, 0f, true);
                    break;
                case ShootingPattern.Aimed:
                    Vector3 target = GetPlayerPosition();
                    BulletPattern.Aimed(origin, target, true);
                    break;
                case ShootingPattern.Circle:
                    BulletPattern.Circle(origin, _circleCount, true);
                    break;
                case ShootingPattern.Spread:
                    BulletPattern.Spread(origin, _spreadCount, _spreadArc, true);
                    break;
                case ShootingPattern.None:
                default:
                    break;
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.EnemyShoot, Random.Range(0.9f, 1.1f), 0.5f);
        }

        private Vector3 GetPlayerPosition()
        {
            if (GameManager.Instance != null)
            {
                GameObject p = GameManager.Instance.GetPlayer();
                if (p != null) return p.transform.position;
            }
            GameObject tagged = GameObject.FindGameObjectWithTag(GameConstants.TAG_PLAYER);
            return tagged != null ? tagged.transform.position : origin_fallback();
        }

        private Vector3 origin_fallback()
        {
            return new Vector3(transform.position.x, GameConstants.CAMERA_BOTTOM, 0f);
        }
        #endregion

        #region Configuration
        /// <summary>Configures pattern parameters at spawn time.</summary>
        public void Configure(ShootingPattern pattern, float fireRate)
        {
            _pattern = pattern;
            _fireRate = fireRate;
        }
        #endregion
    }
}
