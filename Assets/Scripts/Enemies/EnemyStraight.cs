using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemyStraight : EnemyBase
    {
        protected override Vector2 GetMovementDirection()
        {
            return Vector2.down;
        }
    }
}
