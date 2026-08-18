using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Enemy movement patterns.</summary>
    public enum MovementPattern
    {
        StraightDown,
        Sine,
        Zigzag,
        Circle,
        Dive
    }

    /// <summary>
    /// Drives an enemy along a chosen time-based movement pattern and destroys the
    /// enemy once it leaves the bottom of the screen.
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        public MovementPattern pattern = MovementPattern.StraightDown;

        [Tooltip("Base downward/travel speed in world units per second.")]
        public float speed = 3f;

        [Tooltip("Horizontal amplitude for Sine / Zigzag / Circle patterns.")]
        public float amplitude = 2f;

        [Tooltip("Oscillation frequency for Sine / Zigzag / Circle patterns.")]
        public float frequency = 2f;

        [Tooltip("World Y below which the enemy despawns.")]
        public float despawnY = -6f;

        private float _elapsed;
        private float _startX;
        private float _startY;
        private Vector3 _diveTarget;
        private bool _diveTargetSet;

        private void OnEnable()
        {
            _elapsed = 0f;
            _startX = transform.position.x;
            _startY = transform.position.y;
            _diveTargetSet = false;
        }

        /// <summary>Sets the pattern and speed at spawn time.</summary>
        public void Configure(MovementPattern newPattern, float newSpeed)
        {
            pattern = newPattern;
            speed = newSpeed;
            _startX = transform.position.x;
            _startY = transform.position.y;
            _elapsed = 0f;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            transform.position = Evaluate(_elapsed);

            if (transform.position.y < despawnY)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Returns the world position of the enemy at travel time <paramref name="t"/>.
        /// Pure function of the start position, pattern and tuning values.
        /// </summary>
        public Vector3 Evaluate(float t)
        {
            float y = _startY - speed * t;
            float x = _startX;

            switch (pattern)
            {
                case MovementPattern.Sine:
                    x = _startX + Mathf.Sin(t * frequency) * amplitude;
                    break;

                case MovementPattern.Zigzag:
                    // Triangle wave between -amplitude and +amplitude.
                    float phase = Mathf.PingPong(t * frequency, 2f) - 1f;
                    x = _startX + phase * amplitude;
                    break;

                case MovementPattern.Circle:
                    x = _startX + Mathf.Cos(t * frequency) * amplitude;
                    y = _startY - speed * t + Mathf.Sin(t * frequency) * amplitude;
                    break;

                case MovementPattern.Dive:
                    if (!_diveTargetSet)
                    {
                        var player = GameObject.FindWithTag("Player");
                        _diveTarget = player != null
                            ? player.transform.position
                            : new Vector3(_startX, despawnY, 0f);
                        _diveTargetSet = true;
                    }
                    // Ease toward the target X while accelerating downward.
                    float dive = Mathf.Clamp01(t * 0.6f);
                    x = Mathf.Lerp(_startX, _diveTarget.x, dive);
                    y = _startY - speed * t * (1f + t * 0.5f);
                    break;

                case MovementPattern.StraightDown:
                default:
                    x = _startX;
                    break;
            }

            return new Vector3(x, y, transform.position.z);
        }
    }
}
