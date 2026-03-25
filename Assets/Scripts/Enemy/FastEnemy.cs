// =============================================================================
// FastEnemy.cs — Quick enemy with zigzag movement
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Fast enemy: moves quickly with a sinusoidal zigzag pattern.
    /// </summary>
    public class FastEnemy : EnemyBase
    {
        [Header("Zigzag Settings")]
        [SerializeField] private float zigzagAmplitude = 3f;
        [SerializeField] private float zigzagFrequency = 2f;

        private float startX;
        private float timeOffset;

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Fast;
        }

        protected override void Start()
        {
            base.Start();
            startX = transform.position.x;
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        protected override void Move()
        {
            // Move down
            float newY = transform.position.y - moveSpeed * Time.deltaTime;
            // Zigzag horizontally
            float newX = startX + Mathf.Sin((Time.time + timeOffset) * zigzagFrequency) * zigzagAmplitude;
            transform.position = new Vector3(newX, newY, 0f);
        }
    }
}
