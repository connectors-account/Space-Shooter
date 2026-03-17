using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Enemy that moves in a zig-zag pattern
    /// </summary>
    public class ZigZagEnemy : EnemyBase
    {
        [Header("ZigZag Settings")]
        [SerializeField] private float horizontalSpeed = 3f;
        [SerializeField] private float zigZagInterval = 1.5f;
        
        private float direction = 1f;
        private float nextDirectionChangeTime;
        private Camera mainCamera;
        private float screenBoundX;
        
        protected override void Start()
        {
            base.Start();
            mainCamera = Camera.main;
            
            if (mainCamera != null)
            {
                Vector3 screenBounds = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
                screenBoundX = screenBounds.x - 1f;
            }
            else
            {
                screenBoundX = 8f;
            }
            
            nextDirectionChangeTime = Time.time + zigZagInterval;
            direction = Random.value > 0.5f ? 1f : -1f;
        }
        
        protected override void Move()
        {
            // Vertical movement
            transform.position += Vector3.down * moveSpeed * Time.deltaTime;
            
            // Horizontal zig-zag
            transform.position += Vector3.right * horizontalSpeed * direction * Time.deltaTime;
            
            // Change direction at interval or at screen bounds
            if (Time.time >= nextDirectionChangeTime)
            {
                direction *= -1f;
                nextDirectionChangeTime = Time.time + zigZagInterval;
            }
            
            // Bounce off screen edges
            if (Mathf.Abs(transform.position.x) > screenBoundX)
            {
                direction *= -1f;
                Vector3 clampedPos = transform.position;
                clampedPos.x = Mathf.Clamp(clampedPos.x, -screenBoundX, screenBoundX);
                transform.position = clampedPos;
            }
        }
    }
}
