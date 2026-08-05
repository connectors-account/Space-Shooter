using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Fast, fragile enemy that dives straight down and does not shoot.
    /// HP 1, score 100.
    /// </summary>
    public class EnemyDrone : EnemyBase
    {
        protected override void Awake()
        {
            maxHp = 1;
            scoreValue = 100;
            moveSpeed = 5.5f;
            shootInterval = float.PositiveInfinity; // never shoots
            powerUpDropChance = 0.08f;
            base.Awake();
        }

        protected override void AssignSprite()
        {
            if (Renderer.sprite == null)
                Renderer.sprite = SpriteGenerator.CreateEnemyDroneSprite();
        }

        protected override void Move()
        {
            transform.position += Vector3.down * (moveSpeed * DifficultyMultiplier * Time.deltaTime);
        }
    }
}
