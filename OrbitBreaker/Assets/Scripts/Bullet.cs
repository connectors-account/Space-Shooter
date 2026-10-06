using UnityEngine;

namespace OrbitBreaker
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Bullet : MonoBehaviour
    {
        private GameController game;
        private Rigidbody2D body;
        private Vector2 velocity;
        private float life;
        private bool friendly, consumed;

        private void Awake() { body = GetComponent<Rigidbody2D>(); }

        public void Initialize(GameController controller, Vector2 direction, float speed, bool isFriendly)
        {
            game = controller;
            friendly = isFriendly;
            velocity = direction.normalized * speed;
            body.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            body.velocity = velocity;
        }

        private void FixedUpdate()
        {
            if (game == null || consumed) return;
            body.velocity = game.Phase == GamePhase.Playing ? velocity : Vector2.zero;
            life += Time.fixedDeltaTime;
            if (life > 8f || Mathf.Abs(body.position.x) > 10f || Mathf.Abs(body.position.y) > 6.5f) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || game == null || game.Phase != GamePhase.Playing) return;
            if (friendly && other.TryGetComponent(out Enemy enemy) && enemy.IsAlive)
            {
                Consume();
                enemy.Hit();
            }
            else if (!friendly && other.TryGetComponent(out PlayerController player))
            {
                Consume();
                player.Hit();
            }
        }

        private void Consume()
        {
            consumed = true;
            GetComponent<Collider2D>().enabled = false;
            body.velocity = Vector2.zero;
            Destroy(gameObject);
        }
    }
}
