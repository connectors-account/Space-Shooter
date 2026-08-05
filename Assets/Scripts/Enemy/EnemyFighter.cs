using UnityEngine;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Descends while weaving in a sine wave, firing a downward spread every
    /// couple of seconds. HP 3, score 250.
    /// </summary>
    public class EnemyFighter : EnemyBase
    {
        [SerializeField] private float waveAmplitude = 2f;
        [SerializeField] private float waveFrequency = 2f;

        private float _startX;
        private float _phase;

        protected override void Awake()
        {
            maxHp = 3;
            scoreValue = 250;
            moveSpeed = 2.8f;
            shootInterval = 2f;
            powerUpDropChance = 0.18f;
            base.Awake();
        }

        protected override void AssignSprite()
        {
            if (Renderer.sprite == null)
                Renderer.sprite = SpriteGenerator.CreateEnemyFighterSprite();
        }

        protected override void SetupPattern()
        {
            var spread = gameObject.GetComponent<BulletPatternSpread>();
            if (spread == null) spread = gameObject.AddComponent<BulletPatternSpread>();
            spread.Configure(3, 40f);
            Pattern = spread;
        }

        public override void Initialise(string poolKey, Transform player, float difficultyMultiplier)
        {
            base.Initialise(poolKey, player, difficultyMultiplier);
            _startX = transform.position.x;
            _phase = Random.Range(0f, Mathf.PI * 2f);
        }

        protected override void Move()
        {
            _phase += waveFrequency * Time.deltaTime;
            float x = _startX + Mathf.Sin(_phase) * waveAmplitude;
            float y = transform.position.y - moveSpeed * DifficultyMultiplier * Time.deltaTime;
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }
}
