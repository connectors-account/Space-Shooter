using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Basic enemy that moves straight down and shoots occasionally
    /// </summary>
    public class BasicEnemy : EnemyBase
    {
        [Header("Basic Enemy Settings")]
        [SerializeField] private float horizontalWobble = 0f;
        [SerializeField] private float wobbleSpeed = 2f;
        
        private float startX;
        private float elapsedTime;
        
        protected override void Start()
        {
            base.Start();
            startX = transform.position.x;
        }
        
        protected override void Move()
        {
            elapsedTime += Time.deltaTime;
            
            float xOffset = 0f;
            if (horizontalWobble > 0)
            {
                xOffset = Mathf.Sin(elapsedTime * wobbleSpeed) * horizontalWobble;
            }
            
            Vector3 newPosition = transform.position;
            newPosition.y -= moveSpeed * Time.deltaTime;
            newPosition.x = startX + xOffset;
            
            transform.position = newPosition;
        }
    }
}
