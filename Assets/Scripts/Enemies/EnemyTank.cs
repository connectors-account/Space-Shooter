// ============================================================================
// EnemyTank.cs — Slow, high-HP enemy that shoots frequently
// ============================================================================
using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemyTank : EnemyBase
    {
        // EnemyTank is configured via Inspector:
        //   maxHealth = 5, moveSpeed = 1.5, canShoot = true, shootInterval = 1.5
        //   scoreValue = 500

        protected override void Move()
        {
            transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime));
        }
    }
}
