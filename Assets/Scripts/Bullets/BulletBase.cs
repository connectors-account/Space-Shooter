using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Identifies which pool a bullet belongs to.</summary>
    public enum BulletType
    {
        Player,
        Enemy
    }

    /// <summary>
    /// Base class for all projectiles. Moves via a kinematic-style Rigidbody2D
    /// velocity, auto-returns to the pool after a lifetime or when it leaves the
    /// screen, and forwards trigger collisions to <see cref="OnHit"/>.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class BulletBase : MonoBehaviour
    {
        [Tooltip("Travel speed in world units per second.")]
        public float speed = 10f;

        [Tooltip("Damage dealt on hit.")]
        public int damage = 1;

        [Tooltip("Seconds before the bullet auto-returns to the pool.")]
        public float lifeTime = 5f;

        /// <summary>Which pool this bullet is served from.</summary>
        public BulletType Type { get; set; }

        /// <summary>Current normalized travel direction.</summary>
        public Vector2 Direction { get; private set; }

        private Rigidbody2D _rb;
        private float _spawnTime;

        protected Rigidbody2D Body
        {
            get
            {
                if (_rb == null) _rb = GetComponent<Rigidbody2D>();
                return _rb;
            }
        }

        /// <summary>
        /// Positions the bullet, sets its direction/velocity and resets its lifetime.
        /// Called by the <see cref="BulletPool"/> when a bullet is served.
        /// </summary>
        public virtual void Launch(Vector3 position, Vector2 direction)
        {
            transform.position = position;
            Direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;

            if (Body != null)
            {
                Body.gravityScale = 0f;
                Body.velocity = Direction * speed;
            }

            // Face travel direction (sprites are drawn pointing up).
            float angle = Vector2.SignedAngle(Vector2.up, Direction);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            _spawnTime = Time.time;
        }

        protected virtual void Update()
        {
            if (Time.time - _spawnTime >= lifeTime)
            {
                ReturnToPool();
                return;
            }

            if (IsOffScreen())
            {
                ReturnToPool();
            }
        }

        /// <summary>Returns the bullet to its pool (or deactivates it if no pool exists).</summary>
        public void ReturnToPool()
        {
            if (Body != null) Body.velocity = Vector2.zero;

            if (BulletPool.Instance != null)
            {
                BulletPool.Instance.ReturnBullet(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private bool IsOffScreen()
        {
            Camera cam = Camera.main;
            if (cam == null || !cam.orthographic) return false;

            Vector3 vp = cam.WorldToViewportPoint(transform.position);
            const float margin = 0.1f;
            return vp.x < -margin || vp.x > 1f + margin || vp.y < -margin || vp.y > 1f + margin;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            OnHit(other);
        }

        /// <summary>Handles what happens when the bullet overlaps another collider.</summary>
        protected abstract void OnHit(Collider2D other);
    }
}
