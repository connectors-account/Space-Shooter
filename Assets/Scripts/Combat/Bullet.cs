using SpaceShooter.Core;
using SpaceShooter.Enemy;
using SpaceShooter.Player;
using UnityEngine;

namespace SpaceShooter.Combat
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5f;

        private BulletOwner owner;
        private int damage;
        private Vector2 direction;
        private float speed;
        private Camera cam;

        public void Initialize(BulletOwner bulletOwner, int bulletDamage, Vector2 bulletDirection, float bulletSpeed)
        {
            owner = bulletOwner;
            damage = bulletDamage;
            direction = bulletDirection.normalized;
            speed = bulletSpeed;
            gameObject.layer = owner == BulletOwner.Player
                ? GameLayers.GetLayerOrDefault(GameLayers.PlayerBullet)
                : GameLayers.GetLayerOrDefault(GameLayers.EnemyBullet);
        }

        private void Awake()
        {
            cam = Camera.main;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            if (cam != null && ScreenBounds.IsOutside(cam, transform.position, 0.8f))
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (owner == BulletOwner.Player)
            {
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
            else
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.TakeDamage(damage);
                    Destroy(gameObject);
                }
            }
        }
    }
}
