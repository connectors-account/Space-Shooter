using UnityEngine;

namespace Starfall
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class Projectile : MonoBehaviour
    {
        public bool hostile;
        bool consumed;
        float expires;
        public static void Spawn(Projectile prefab, Vector3 position, Vector2 direction, float speed)
        {
            var bullet = Instantiate(prefab, position, Quaternion.identity, StarGame.Instance.Actors);
            bullet.transform.up = direction;
            bullet.GetComponent<Rigidbody2D>().velocity = direction.normalized * speed;
            bullet.expires = Time.time + 9;
        }
        void Update()
        {
            Vector3 p = transform.position;
            if (Time.time > expires || Mathf.Abs(p.x) > 10 || Mathf.Abs(p.y) > 6.5f) Destroy(gameObject);
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed || StarGame.Instance.State != RunState.Playing) return;
            if (hostile)
            {
                var player = other.GetComponent<PlayerShip>();
                if (player == null) return;
                consumed = true; player.Damage();
            }
            else
            {
                var enemy = other.GetComponent<EnemyShip>();
                if (enemy == null) return;
                consumed = true; enemy.Hit();
            }
            GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject);
        }
    }
}
