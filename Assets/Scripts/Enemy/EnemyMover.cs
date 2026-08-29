using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Movement patterns for enemies.
    /// </summary>
    public enum MovementPattern
    {
        StraightDown,
        SineWave,
        ZigZag,
        CircleIn,
        Dive
    }

    /// <summary>
    /// Moves an enemy according to a selected pattern and destroys it once it exits
    /// the bottom of the screen.
    /// </summary>
    public class EnemyMover : MonoBehaviour
    {
        [Header("Pattern")]
        [SerializeField] private MovementPattern pattern = MovementPattern.StraightDown;
        [SerializeField] private float moveSpeed = 3f;

        [Header("Wave Parameters")]
        [SerializeField] private float amplitude = 2f;
        [SerializeField] private float frequency = 2f;

        [Header("Dive")]
        [SerializeField] private float diveSpeedMultiplier = 2.5f;

        private float _startX;
        private float _startTime;
        private float _bottomBound;
        private Transform _target;
        private bool _initialized;

        public void Initialize(MovementPattern newPattern, float speed)
        {
            pattern = newPattern;
            moveSpeed = speed;
            _startX = transform.position.x;
            _startTime = Time.time;
            _initialized = true;

            Camera cam = Camera.main;
            if (cam != null)
            {
                _bottomBound = cam.ViewportToWorldPoint(Vector3.zero).y - 1.5f;
            }
            else
            {
                _bottomBound = -10f;
            }

            var playerObj = GameObject.FindGameObjectWithTag(SpaceShooter.Utilities.Constants.Tags.Player);
            if (playerObj != null)
            {
                _target = playerObj.transform;
            }
        }

        public void SetPattern(MovementPattern newPattern)
        {
            pattern = newPattern;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        private void Start()
        {
            if (!_initialized)
            {
                Initialize(pattern, moveSpeed);
            }
        }

        private void Update()
        {
            float t = Time.time - _startTime;
            Vector3 pos = transform.position;

            switch (pattern)
            {
                case MovementPattern.StraightDown:
                    pos.y -= moveSpeed * Time.deltaTime;
                    break;

                case MovementPattern.SineWave:
                    pos.y -= moveSpeed * Time.deltaTime;
                    pos.x = _startX + Mathf.Sin(t * frequency) * amplitude;
                    break;

                case MovementPattern.ZigZag:
                    pos.y -= moveSpeed * Time.deltaTime;
                    // Triangle wave: sharp back-and-forth.
                    float phase = Mathf.PingPong(t * frequency, 2f) - 1f;
                    pos.x = _startX + phase * amplitude;
                    break;

                case MovementPattern.CircleIn:
                    pos.y -= moveSpeed * 0.6f * Time.deltaTime;
                    pos.x = _startX + Mathf.Cos(t * frequency) * amplitude * Mathf.Max(0.2f, 1f - t * 0.15f);
                    break;

                case MovementPattern.Dive:
                    if (_target != null && t < 1.5f)
                    {
                        Vector3 dir = (_target.position - transform.position).normalized;
                        pos += dir * (moveSpeed * diveSpeedMultiplier * Time.deltaTime);
                    }
                    else
                    {
                        pos.y -= moveSpeed * diveSpeedMultiplier * Time.deltaTime;
                    }
                    break;
            }

            transform.position = pos;

            if (transform.position.y < _bottomBound)
            {
                Destroy(gameObject);
            }
        }
    }
}
