// ============================================================================
// EnemyStraight.cs — Basic enemy that flies straight down
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemyStraight : EnemyBase
    {
        protected override void Move()
        {
            transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime));
        }
    }
}
