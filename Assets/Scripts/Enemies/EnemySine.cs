using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemySine : EnemyBase
    {
        [SerializeField] private float horizontalFrequency = 2f;
        [SerializeField] private float horizontalAmplitude = 1.8f;

        private float spawnTime;

        protected override void Awake()
        {
            base.Awake();
            spawnTime = Time.time;
        }

        protected override Vector2 GetMovementDirection()
        {
            float t = Time.time - spawnTime;
            float horizontal = Mathf.Sin(t * horizontalFrequency) * horizontalAmplitude;
            return new Vector2(horizontal, -1f);
        }
    }
}
