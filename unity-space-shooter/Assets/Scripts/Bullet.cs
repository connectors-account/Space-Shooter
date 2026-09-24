using UnityEngine;

namespace Starward
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : MonoBehaviour
    {
        GameManager game;
        Rigidbody2D body;
        Vector2 velocity;
        bool friendly, spent;
        float expires;
        // Allocated once per shot, not on each physics tick.
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        ContactFilter2D filter;
        void Awake() { body = GetComponent<Rigidbody2D>(); }
        public void Initialize(GameManager owner, Vector2 direction, bool playerOwned)
        {
            game = owner;
            friendly = playerOwned;
            velocity = direction.normalized * (friendly ? 18f : 5f);
            expires = Time.time + 6;
            filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(1 << (friendly ? 9 : 8));
            transform.rotation = Quaternion.Euler(0, 0, -Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg);
        }
        void FixedUpdate()
        {
            if (game == null || !game.IsPlaying || spent) return;
            Vector2 step = velocity * Time.fixedDeltaTime;
            // Swept cast catches fast shots crossing a thin collider between physics steps.
            int count = Physics2D.CircleCast(body.position, .12f, step.normalized, filter, hits, step.magnitude);
            for (int i = 0; i < count && !spent; i++) TryHit(hits[i].collider);
            if (spent) return;
            body.MovePosition(body.position + step);
            if (Time.time > expires || Mathf.Abs(body.position.x) > 11 || Mathf.Abs(body.position.y) > 7) Retire();
        }
        void OnTriggerEnter2D(Collider2D other) { TryHit(other); }
        void TryHit(Collider2D other)
        {
            if (spent || game == null || !game.IsPlaying || other == null) return;
            if (friendly)
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy == null) return;
                enemy.Hit(1);
            }
            else
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null) return;
                player.Hit(1);
            }
            Retire();
        }
        void Retire()
        {
            spent = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
