using UnityEngine;

namespace SpaceShooter.Combat
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed = 15f;
        [SerializeField] private int damage = 10;
        [SerializeField] private bool isPlayerBullet = true;
        [SerializeField] private float lifetime = 5f;

        private Rigidbody2D rb;
        private Vector2 direction;

        public int Damage => damage;
        public bool IsPlayerBullet => isPlayerBullet;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        private void Start()
        {
            Destroy(gameObject, lifetime);
            SetTag();
        }

        public void Initialize(Vector2 direction, bool isPlayerBullet, int damage)
        {
            this.direction = direction.normalized;
            this.isPlayerBullet = isPlayerBullet;
            this.damage = damage;
            SetTag();
            
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void SetTag()
        {
            gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
        }

        private void FixedUpdate()
        {
            rb.velocity = direction * speed;
        }

        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
