using UnityEngine;

namespace Starfall
{
    public enum PowerKind { Repair, Rapid, Spread }
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pickup : MonoBehaviour
    {
        public PowerKind kind;
        bool consumed;
        void Start() { GetComponent<Rigidbody2D>().velocity = Vector2.down * 1.5f; }
        void Update()
        {
            if (transform.position.y < -6) Destroy(gameObject);
            transform.localScale = Vector3.one * (1 + 0.08f * Mathf.Sin(Time.time * 5));
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || StarGame.Instance.State != RunState.Playing) return;
            var player = other.GetComponent<PlayerShip>();
            if (player == null) return;
            consumed = true; player.Collect(kind); Destroy(gameObject);
        }
    }
}
