using UnityEngine;

namespace OrbitBreaker
{
    public enum PowerupKind { Repair, Rapid, Triple }

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Powerup : MonoBehaviour
    {
        public PowerupKind kind;
        private GameController game;
        private Rigidbody2D body;
        private Vector3 initialScale;
        private float age;
        private bool collected;

        private void Awake() { body = GetComponent<Rigidbody2D>(); initialScale = transform.localScale; }
        public void Initialize(GameController controller) { game = controller; }

        private void FixedUpdate()
        {
            if (game == null || game.Phase != GamePhase.Playing) { body.velocity = Vector2.zero; return; }
            body.velocity = Vector2.down * 1.1f;
            age += Time.fixedDeltaTime;
            transform.localScale = initialScale * (1f + .08f * Mathf.Sin(age * 5f));
            if (age > 14f || body.position.y < -5.8f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || game == null || game.Phase != GamePhase.Playing) return;
            if (!other.TryGetComponent(out PlayerController player)) return;
            collected = true;
            GetComponent<Collider2D>().enabled = false;
            player.Collect(kind);
            Destroy(gameObject);
        }
    }
}
