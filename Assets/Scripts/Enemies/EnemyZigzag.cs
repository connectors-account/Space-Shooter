// ============================================================================
// EnemyZigzag.cs — Moves downward while weaving left and right
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemyZigzag : EnemyBase
    {
        [Header("Zigzag")]
        [SerializeField] private float zigzagFrequency = 3f;   // oscillations per second
        [SerializeField] private float zigzagAmplitude = 3f;    // lateral distance

        private float _startX;
        private float _time;

        protected override void OnEnable()
        {
            base.OnEnable();
            _startX = transform.position.x;
            _time = 0f;
        }

        protected override void Move()
        {
            _time += Time.deltaTime;

            float newY = transform.position.y - moveSpeed * Time.deltaTime;
            float newX = _startX + Mathf.Sin(_time * zigzagFrequency * Mathf.PI * 2f) * zigzagAmplitude;

            transform.position = new Vector3(newX, newY, 0f);
        }
    }
}
