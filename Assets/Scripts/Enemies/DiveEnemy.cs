using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Enemy that dives toward the player
    /// </summary>
    public class DiveEnemy : EnemyBase
    {
        [Header("Dive Settings")]
        [SerializeField] private float diveSpeed = 8f;
        [SerializeField] private float detectionRange = 5f;
        [SerializeField] private float diveDelay = 1f;
        
        private Transform playerTransform;
        private bool isDiving = false;
        private Vector3 diveDirection;
        private float spawnTime;
        private bool hasStartedDive = false;
        
        protected override void Start()
        {
            base.Start();
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            
            spawnTime = Time.time;
        }
        
        protected override void Move()
        {
            if (!hasStartedDive)
            {
                // Move down slowly until ready to dive
                transform.position += Vector3.down * moveSpeed * 0.5f * Time.deltaTime;
                
                // Start dive after delay
                if (Time.time - spawnTime >= diveDelay)
                {
                    StartDive();
                }
            }
            else if (isDiving)
            {
                // Dive toward target position
                transform.position += diveDirection * diveSpeed * Time.deltaTime;
            }
        }
        
        private void StartDive()
        {
            hasStartedDive = true;
            isDiving = true;
            
            if (playerTransform != null)
            {
                // Calculate direction toward player with prediction
                Vector3 targetPos = playerTransform.position;
                diveDirection = (targetPos - transform.position).normalized;
            }
            else
            {
                diveDirection = Vector3.down;
            }
            
            // Rotate to face dive direction
            float angle = Mathf.Atan2(diveDirection.y, diveDirection.x) * Mathf.Rad2Deg + 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
