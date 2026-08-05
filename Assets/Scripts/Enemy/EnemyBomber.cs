using UnityEngine;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Slow, tough enemy that drifts diagonally and fires aimed shots at the
    /// player every few seconds. Guaranteed to drop a power-up. HP 5, score 400.
    /// </summary>
    public class EnemyBomber : EnemyBase
    {
        [SerializeField] private float horizontalDrift = 1.2f;

        private int _driftDir = 1;

        protected override void Awake()
        {
            maxHp = 5;
            scoreValue = 400;
            moveSpeed = 1.8f;
            shootInterval = 3f;
            guaranteedDrop = true;
            base.Awake();
        }

        protected override void AssignSprite()
        {
            if (Renderer.sprite == null)
                Renderer.sprite = SpriteGenerator.CreateEnemyBomberSprite();
        }

        protected override void SetupPattern()
        {
            var aimed = gameObject.GetComponent<BulletPatternAimed>();
            if (aimed == null) aimed = gameObject.AddComponent<BulletPatternAimed>();
            Pattern = aimed;
        }

        public override void Initialise(string poolKey, Transform player, float difficultyMultiplier)
        {
            base.Initialise(poolKey, player, difficultyMultiplier);
            _driftDir = transform.position.x < 0f ? 1 : -1;
        }

        protected override void Move()
        {
            float x = transform.position.x + _driftDir * horizontalDrift * DifficultyMultiplier * Time.deltaTime;
            float y = transform.position.y - moveSpeed * DifficultyMultiplier * Time.deltaTime;

            // Bounce off horizontal screen edges.
            if (Cam != null && Cam.orthographic)
            {
                float halfW = Cam.orthographicSize * Cam.aspect;
                float cx = Cam.transform.position.x;
                if (x > cx + halfW - 0.6f) _driftDir = -1;
                else if (x < cx - halfW + 0.6f) _driftDir = 1;
            }

            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
