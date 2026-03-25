// =============================================================================
// BasicEnemy.cs — Standard enemy that moves straight down
// =============================================================================
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>
    /// Basic enemy: moves straight down, fires single shots.
    /// </summary>
    public class BasicEnemy : EnemyBase
    {
        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.Basic;
        }

        protected override void Move()
        {
            transform.Translate(Vector2.down * moveSpeed * Time.deltaTime, Space.World);
        }
    }
}
