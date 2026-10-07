using UnityEngine;

namespace StellarShooter
{
    public enum PickupKind { Spread, Repair, Shield }
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Pickup : MonoBehaviour
    {
        public PickupKind kind;
        private GameSession game;
        private Rigidbody2D body;
        private bool consumed;
        private float age;
        private void Awake() { body = GetComponent<Rigidbody2D>(); }
        public void Initialize(GameSession session) { game = session; }
        private void FixedUpdate()
        {
            if (game == null || !game.IsPlaying) return;
            body.MovePosition(body.position + Vector2.down * (.95f * Time.fixedDeltaTime));
        }
        private void Update()
        {
            if (game == null || !game.IsPlaying) return;
            age += Time.deltaTime;
            transform.localScale = Vector3.one * (.68f + Mathf.Sin(age * 5) * .045f);
            if (transform.position.y < -5.7f) Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || consumed || !game.IsPlaying) return;
            consumed = true;
            player.ApplyPickup(kind);
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject);
        }
    }
}
