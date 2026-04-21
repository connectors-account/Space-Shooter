using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class ZigZagEnemy : EnemyBase
    {
        [SerializeField] private float zigZagFrequency = 3f;
        [SerializeField] private float zigZagAmplitude = 1.4f;

        private float spawnX;

        protected override void OnEnable()
        {
            base.OnEnable();
            spawnX = transform.position.x;
        }

        protected override void Move()
        {
            float y = transform.position.y - (moveSpeed * Time.deltaTime);
            float xOffset = Mathf.Sin(Time.time * zigZagFrequency) * zigZagAmplitude;
            transform.position = new Vector3(spawnX + xOffset, y, transform.position.z);

            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }
    }
}
