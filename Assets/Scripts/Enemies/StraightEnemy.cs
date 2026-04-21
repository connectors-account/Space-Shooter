using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class StraightEnemy : EnemyBase
    {
        protected override void Move()
        {
            transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime), Space.World);
            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }
    }
}
