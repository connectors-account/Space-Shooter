using UnityEngine;

namespace NovaGuard
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class Bullet : MonoBehaviour
    {
        private Rigidbody2D body;
        private bool friendly, spent;
        private float expires;

        public void Launch(Vector2 direction, float speed, bool isFriendly)
        {
            body = GetComponent<Rigidbody2D>();
            friendly = isFriendly;
            expires = Time.time + 7;
            direction = direction.normalized;
            body.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            body.velocity = direction * speed;
        }

        private void Update()
        {
            Vector3 p = transform.position;
            if (Time.time > expires || Mathf.Abs(p.x) > 18 || Mathf.Abs(p.y) > 12) Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (spent || !GameController.Instance.IsPlaying) return;
            if (friendly)
            {
                EnemyShip enemy = other.GetComponent<EnemyShip>();
                if (enemy == null) return;
                spent = true;
                enemy.TakeDamage(1);
            }
            else
            {
                PlayerShip player = other.GetComponent<PlayerShip>();
                if (player == null) return;
                spent = true;
                player.TakeDamage(1);
            }
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}
