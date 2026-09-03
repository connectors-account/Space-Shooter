using System.Collections;
using UnityEngine;
using SpaceShooter.Core;
using SpaceShooter.Bullets;
using SpaceShooter.Enemy;

namespace SpaceShooter.Player
{
    /// <summary>Firing patterns the player ship can use.</summary>
    public enum ShootPattern { Single, Double, Triple, Spread5, Laser }

    /// <summary>
    /// Handles the player's weapons. Fires the active pattern on demand with a
    /// per-pattern cooldown, pulling bullets from the BulletPool. Supports a laser
    /// mode that deals continuous raycast damage.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        #region Fields
        [SerializeField] private ShootPattern _currentPattern = ShootPattern.Single;
        [SerializeField] private Transform _muzzle;

        private float _cooldownTimer;
        private bool _laserActive;
        private Coroutine _laserRoutine;
        #endregion

        #region Properties
        public ShootPattern CurrentPattern => _currentPattern;
        #endregion

        #region Unity Lifecycle
        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }
        #endregion

        #region Pattern Control
        /// <summary>Sets the active firing pattern.</summary>
        public void SetPattern(ShootPattern pattern)
        {
            // Cancel an active laser if switching away.
            if (_laserActive && pattern != ShootPattern.Laser)
                StopLaser();
            _currentPattern = pattern;
        }

        /// <summary>Reverts to the default single-shot weapon.</summary>
        public void ResetToDefault()
        {
            SetPattern(ShootPattern.Single);
        }
        #endregion

        #region Firing
        private Vector3 MuzzlePosition => _muzzle != null ? _muzzle.position : transform.position + Vector3.up * 0.5f;

        /// <summary>Attempts to fire the active pattern, respecting cooldown.</summary>
        public void TryFire()
        {
            if (_currentPattern == ShootPattern.Laser)
            {
                if (!_laserActive)
                    ActivateLaser();
                return;
            }

            if (_cooldownTimer > 0f) return;

            switch (_currentPattern)
            {
                case ShootPattern.Single: FireSingle(); break;
                case ShootPattern.Double: FireDouble(); break;
                case ShootPattern.Triple: FireTriple(); break;
                case ShootPattern.Spread5: FireSpread5(); break;
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.Shoot, Random.Range(0.95f, 1.05f));
        }

        private void FireSingle()
        {
            BulletPattern.Straight(MuzzlePosition, 0f, false);
            _cooldownTimer = GameConstants.COOLDOWN_SINGLE;
        }

        private void FireDouble()
        {
            Vector3 left = MuzzlePosition + Vector3.left * 0.25f;
            Vector3 right = MuzzlePosition + Vector3.right * 0.25f;
            BulletPattern.Straight(left, 0f, false);
            BulletPattern.Straight(right, 0f, false);
            _cooldownTimer = GameConstants.COOLDOWN_DOUBLE;
        }

        private void FireTriple()
        {
            BulletPattern.Straight(MuzzlePosition, 0f, false);
            BulletPattern.Straight(MuzzlePosition, -15f, false);
            BulletPattern.Straight(MuzzlePosition, 15f, false);
            _cooldownTimer = GameConstants.COOLDOWN_TRIPLE;
        }

        private void FireSpread5()
        {
            float[] angles = { -30f, -15f, 0f, 15f, 30f };
            foreach (float a in angles)
                BulletPattern.Straight(MuzzlePosition, a, false);
            _cooldownTimer = GameConstants.COOLDOWN_SPREAD5;
        }
        #endregion

        #region Laser
        /// <summary>Starts the timed continuous-damage laser.</summary>
        public void ActivateLaser()
        {
            if (_laserActive) return;
            _currentPattern = ShootPattern.Laser;
            _laserActive = true;
            _laserRoutine = StartCoroutine(LaserRoutine());
        }

        private void StopLaser()
        {
            if (_laserRoutine != null) StopCoroutine(_laserRoutine);
            _laserActive = false;
            HideLaserBeam();
        }

        private LineRenderer _beam;

        private IEnumerator LaserRoutine()
        {
            float elapsed = 0f;
            EnsureBeam();

            while (elapsed < GameConstants.LASER_DURATION)
            {
                Vector3 origin = MuzzlePosition;
                Vector3 end = new Vector3(origin.x, GameConstants.CAMERA_TOP + 1f, 0f);

                // Continuous raycast damage against enemies.
                int enemyMask = 1 << GameConstants.LAYER_ID_ENEMY;
                RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.up, (end - origin).magnitude, enemyMask);
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider == null) continue;
                    EnemyBase enemy = hit.collider.GetComponent<EnemyBase>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(GameConstants.LASER_DAMAGE_PER_TICK);
                        end = hit.point; // beam stops at first hit visually
                        break;
                    }
                }

                DrawLaserBeam(origin, end);

                if (AudioManager.Instance != null && Mathf.Approximately(elapsed % 0.2f, 0f))
                    AudioManager.Instance.PlaySFX(AudioManager.Instance.Shoot, 1.3f, 0.4f);

                elapsed += GameConstants.COOLDOWN_LASER_TICK;
                yield return new WaitForSeconds(GameConstants.COOLDOWN_LASER_TICK);
            }

            _laserActive = false;
            HideLaserBeam();
            _currentPattern = ShootPattern.Single;
        }

        private void EnsureBeam()
        {
            if (_beam != null) return;
            GameObject go = new GameObject("LaserBeam");
            go.transform.SetParent(transform, false);
            _beam = go.AddComponent<LineRenderer>();
            _beam.material = new Material(Shader.Find("Sprites/Default"));
            _beam.startColor = new Color(1f, 0.2f, 1f, 0.9f);
            _beam.endColor = new Color(1f, 0.6f, 1f, 0.5f);
            _beam.startWidth = 0.25f;
            _beam.endWidth = 0.1f;
            _beam.numCapVertices = 4;
            _beam.sortingOrder = 20;
        }

        private void DrawLaserBeam(Vector3 origin, Vector3 end)
        {
            if (_beam == null) return;
            _beam.enabled = true;
            _beam.positionCount = 2;
            _beam.SetPosition(0, origin);
            _beam.SetPosition(1, end);
        }

        private void HideLaserBeam()
        {
            if (_beam != null) _beam.enabled = false;
        }
        #endregion
    }
}
