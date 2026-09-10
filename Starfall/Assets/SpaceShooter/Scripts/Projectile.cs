using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class Projectile : MonoBehaviour
    {
        public bool friendly;
        public float hitRadius = 0.13f;
        private Rigidbody2D body;
        private Vector2 velocity;
        private float lifetime;
        private bool consumed;
        private ContactFilter2D filter;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[16];

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            filter = new ContactFilter2D { useTriggers = true };
            filter.SetLayerMask(1 << (friendly ? 9 : 8));
        }

        public void Launch(Vector2 direction, float speed)
        {
            velocity = direction.normalized * speed;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
        }

        private void FixedUpdate()
        {
            if (!GameController.Instance.IsPlaying || consumed) return;
            lifetime += Time.fixedDeltaTime;
            Vector2 step = velocity * Time.fixedDeltaTime;
            // Swept shape query prevents fast bullets tunneling through small trigger colliders.
            int count = Physics2D.CircleCast(body.position, hitRadius, velocity.normalized, filter, hits, step.magnitude);
            for (int i = 0; i < count; i++)
                if (Hit(hits[i].collider)) return;
            body.MovePosition(body.position + step);
            if (lifetime > 8f || Mathf.Abs(body.position.y) > 11f || Mathf.Abs(body.position.x) > GameController.Instance.HalfWidth + 2f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other) { Hit(other); }

        private bool Hit(Collider2D other)
        {
            if (consumed || other == null || !GameController.Instance.IsPlaying) return false;
            if (friendly)
            {
                EnemyShip enemy = other.GetComponent<EnemyShip>();
                if (enemy == null) return false;
                enemy.Damage(1);
            }
            else
            {
                PlayerShip player = other.GetComponent<PlayerShip>();
                if (player == null) return false;
                player.Damage(1);
            }
            consumed = true;
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject);
            return true;
        }
    }
}
