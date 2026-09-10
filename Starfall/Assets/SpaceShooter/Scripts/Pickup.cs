using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
    public sealed class Pickup : MonoBehaviour
    {
        public PickupKind kind;
        private Rigidbody2D body;
        private Vector3 initialScale;
        private float age;
        private bool collected;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            initialScale = transform.localScale;
        }

        private void FixedUpdate()
        {
            if (!GameController.Instance.IsPlaying || collected) return;
            age += Time.fixedDeltaTime;
            body.MovePosition(body.position + Vector2.down * (1.8f * Time.fixedDeltaTime));
            transform.localScale = initialScale * (1f + 0.08f * Mathf.Sin(age * 5f));
            if (body.position.y < -10.2f || age > 16f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerShip player = other.GetComponent<PlayerShip>();
            if (player == null || collected || !GameController.Instance.IsPlaying) return;
            collected = true;
            player.Collect(kind);
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject);
        }
    }
}
