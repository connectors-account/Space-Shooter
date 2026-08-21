using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    public enum MovementPattern
    {
        StraightDown,
        Zigzag,
        Sine,
        CircleStrafe,
        Dive
    }

    /// <summary>
    /// Drives enemy movement using one of several math-based patterns.
    /// Speed is scaled by a per-wave multiplier.
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private MovementPattern pattern = MovementPattern.StraightDown;
        [SerializeField] private float baseSpeed = 3f;

        [Header("Pattern Tuning")]
        [SerializeField] private float zigzagAmplitude = 2.5f;
        [SerializeField] private float zigzagFrequency = 2f;
        [SerializeField] private float sineAmplitude = 2f;
        [SerializeField] private float sineFrequency = 1.5f;
        [SerializeField] private float circleRadius = 1.5f;
        [SerializeField] private float circleSpeed = 3f;
        [SerializeField] private float diveSpeedMultiplier = 2.5f;

        private float _speedMultiplier = 1f;
        private float _elapsed;
        private Vector3 _startPos;
        private float _originX;
        private bool _diveStarted;
        private Transform _diveTarget;

        public void Configure(MovementPattern movementPattern, float speedMultiplier)
        {
            pattern = movementPattern;
            _speedMultiplier = Mathf.Max(0.1f, speedMultiplier);
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0.1f, multiplier);
        }

        private void OnEnable()
        {
            _elapsed = 0f;
            _startPos = transform.position;
            _originX = transform.position.x;
            _diveStarted = false;

            var playerObj = GameObject.FindGameObjectWithTag("Player");
            _diveTarget = playerObj != null ? playerObj.transform : null;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float speed = baseSpeed * _speedMultiplier;
            Vector3 pos = transform.position;

            switch (pattern)
            {
                case MovementPattern.StraightDown:
                    pos.y -= speed * Time.deltaTime;
                    break;

                case MovementPattern.Zigzag:
                    pos.y -= speed * Time.deltaTime;
                    // Triangle wave for sharp zigzag.
                    float tri = Mathf.PingPong(_elapsed * zigzagFrequency, 2f) - 1f;
                    pos.x = _originX + tri * zigzagAmplitude;
                    break;

                case MovementPattern.Sine:
                    pos.y -= speed * Time.deltaTime;
                    pos.x = _originX + Mathf.Sin(_elapsed * sineFrequency * Mathf.PI) * sineAmplitude;
                    break;

                case MovementPattern.CircleStrafe:
                    // Drift downward slowly while orbiting.
                    _startPos.y -= speed * 0.4f * Time.deltaTime;
                    float ang = _elapsed * circleSpeed;
                    pos.x = _startPos.x + Mathf.Cos(ang) * circleRadius;
                    pos.y = _startPos.y + Mathf.Sin(ang) * circleRadius;
                    break;

                case MovementPattern.Dive:
                    if (!_diveStarted && _diveTarget != null)
                    {
                        // Descend normally until roughly the upper third, then dive at the player.
                        if (ScreenBounds.Instance != null &&
                            pos.y <= ScreenBounds.Instance.MaxY - ScreenBounds.Instance.Height * 0.3f)
                        {
                            _diveStarted = true;
                        }
                        pos.y -= speed * Time.deltaTime;
                    }
                    else if (_diveTarget != null)
                    {
                        Vector3 dir = (_diveTarget.position - pos).normalized;
                        pos += dir * speed * diveSpeedMultiplier * Time.deltaTime;
                    }
                    else
                    {
                        pos.y -= speed * Time.deltaTime;
                    }
                    break;
            }

            transform.position = pos;

            // Despawn if it leaves the bottom of the screen.
            if (ScreenBounds.Instance != null &&
                transform.position.y < ScreenBounds.Instance.MinY - 2f)
            {
                var enemy = GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    enemy.Despawn();
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
