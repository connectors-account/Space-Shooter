// =============================================================================
// HomingBullet.cs — Bullet that tracks the nearest enemy or player
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Weapons
{
    /// <summary>
    /// Extended bullet that steers towards the nearest valid target.
    /// Falls back to straight-line travel when no target is found.
    /// </summary>
    public class HomingBullet : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float rotateSpeed = 200f;
        [SerializeField] private float lifetime = 6f;
        [SerializeField] private float searchRadius = 15f;

        private int damage = 1;
        private bool isPlayerBullet = true;
        private Transform target;
        private float spawnTime;

        public int Damage => damage;
        public bool IsPlayerBullet => isPlayerBullet;

        /// <summary>
        /// Initializes the homing bullet.
        /// </summary>
        public void Initialize(float spd, bool playerBullet, int dmg)
        {
            speed = spd;
            isPlayerBullet = playerBullet;
            damage = dmg;
            spawnTime = Time.time;
            gameObject.tag = isPlayerBullet ? "PlayerBullet" : "EnemyBullet";
            FindTarget();
        }

        private void Update()
        {
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Re-acquire target if lost
            if (target == null)
                FindTarget();

            if (target != null)
            {
                Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
                Quaternion targetRot = Quaternion.Euler(0, 0, angle);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }

            transform.Translate(Vector2.up * speed * Time.deltaTime);
        }

        /// <summary>
        /// Finds the nearest valid target (enemies for player bullets, player for enemy bullets).
        /// </summary>
        private void FindTarget()
        {
            string searchTag = isPlayerBullet ? "Enemy" : "Player";
            GameObject[] candidates = GameObject.FindGameObjectsWithTag(searchTag);

            float closest = searchRadius;
            target = null;

            foreach (GameObject go in candidates)
            {
                float dist = Vector2.Distance(transform.position, go.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    target = go.transform;
                }
            }
        }

        private void OnBecameInvisible()
        {
            Destroy(gameObject);
        }
    }
}
