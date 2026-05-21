using UnityEngine;

namespace SpaceShooter.Enemies
{
    /// <summary>
    /// Fast enemy with a zigzag movement pattern.
    /// Higher speed, lower health, higher score.
    /// </summary>
    public class FastEnemy : EnemyBase
    {
        [Header("Zigzag Settings")]
        [SerializeField] private float zigzagAmplitude = 3f;
        [SerializeField] private float zigzagFrequency = 2f;

        private float spawnX;
        private float timeAlive;

        protected override void Awake()
        {
            base.Awake();
            maxHealth = 1;
            moveSpeed = 5f;
            scoreValue = 150;
            canShoot = false;
            powerUpDropChance = 0.12f;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            spawnX = transform.position.x;
            timeAlive = 0f;
        }

        protected override void Move()
        {
            timeAlive += Time.deltaTime;

            // Zigzag horizontally while moving down
            float xOffset = Mathf.Sin(timeAlive * zigzagFrequency * Mathf.PI) * zigzagAmplitude;
            float newX = spawnX + xOffset * Time.deltaTime * zigzagFrequency;

            Vector3 pos = transform.position;
            pos.x += Mathf.Sin(timeAlive * zigzagFrequency * Mathf.PI * 2f) * zigzagAmplitude * Time.deltaTime;
            pos.y -= moveSpeed * Time.deltaTime;
            transform.position = pos;
        }
    }
}
