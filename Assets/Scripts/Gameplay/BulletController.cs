using UnityEngine;

namespace SpaceShooter.Gameplay
{
    public class BulletController : MonoBehaviour
    {
        public enum BulletOwner
        {
            Player,
            Enemy
        }

        private Vector2 direction;
        private float speed;
        private int damage;
        private BulletOwner owner;
        private float lifetime;

        public int Damage => damage;
        public BulletOwner Owner => owner;

        public void Initialize(Vector2 moveDirection, float moveSpeed, int bulletDamage, BulletOwner bulletOwner, float maxLifetime = 4f)
        {
            direction = moveDirection.normalized;
            speed = moveSpeed;
            damage = bulletDamage;
            owner = bulletOwner;
            lifetime = maxLifetime;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            lifetime -= Time.deltaTime;
            if (lifetime <= 0f || Mathf.Abs(transform.position.y) > 7.5f || Mathf.Abs(transform.position.x) > 12f)
            {
                Destroy(gameObject);
            }
        }
    }
}
