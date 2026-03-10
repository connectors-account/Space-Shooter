using UnityEngine;

namespace SpaceShooter.Enemy
{
    public class EnemyFast : EnemyBase
    {
        [Header("Fast Enemy Settings")]
        [SerializeField] private float zigzagAmplitude = 3f;
        [SerializeField] private float zigzagSpeed = 5f;

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Fast;
            maxHealth = 10;
            currentHealth = maxHealth;
            moveSpeed = 6f;
            scoreValue = 150;
            canShoot = false;
            useWaveMovement = true;
            horizontalAmplitude = zigzagAmplitude;
            horizontalFrequency = zigzagSpeed;
        }

        protected override void Move()
        {
            Vector3 newPosition = transform.position;
            newPosition.y -= moveSpeed * Time.deltaTime;
            newPosition.x = startPosition.x + Mathf.Sin(timeAlive * horizontalFrequency) * horizontalAmplitude;
            transform.position = newPosition;
        }
    }
}
