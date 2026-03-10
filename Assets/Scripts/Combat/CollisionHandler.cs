using UnityEngine;
using SpaceShooter.Player;
using SpaceShooter.Enemy;

namespace SpaceShooter.Combat
{
    public class CollisionHandler : MonoBehaviour
    {
        [Header("Layer Settings")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask playerBulletLayer;
        [SerializeField] private LayerMask enemyBulletLayer;

        private void Start()
        {
            SetupCollisionMatrix();
        }

        private void SetupCollisionMatrix()
        {
            int playerLayerIndex = LayerMask.NameToLayer("Player");
            int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
            int playerBulletLayerIndex = LayerMask.NameToLayer("PlayerBullet");
            int enemyBulletLayerIndex = LayerMask.NameToLayer("EnemyBullet");
            int powerUpLayerIndex = LayerMask.NameToLayer("PowerUp");

            if (playerLayerIndex >= 0 && playerBulletLayerIndex >= 0)
                Physics2D.IgnoreLayerCollision(playerLayerIndex, playerBulletLayerIndex, true);

            if (enemyLayerIndex >= 0 && enemyBulletLayerIndex >= 0)
                Physics2D.IgnoreLayerCollision(enemyLayerIndex, enemyBulletLayerIndex, true);

            if (playerBulletLayerIndex >= 0 && enemyBulletLayerIndex >= 0)
                Physics2D.IgnoreLayerCollision(playerBulletLayerIndex, enemyBulletLayerIndex, true);

            if (enemyLayerIndex >= 0 && powerUpLayerIndex >= 0)
                Physics2D.IgnoreLayerCollision(enemyLayerIndex, powerUpLayerIndex, true);
        }
    }
}
