using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class TankEnemy : EnemyBase
    {
        [SerializeField] private float hoverHeight = 2.8f;

        private bool reachedHoverZone;

        protected override void OnEnable()
        {
            base.OnEnable();
            reachedHoverZone = false;
        }

        protected override void Move()
        {
            if (!reachedHoverZone)
            {
                transform.Translate(Vector3.down * (moveSpeed * Time.deltaTime), Space.World);
                if (transform.position.y <= hoverHeight)
                {
                    reachedHoverZone = true;
                }
            }

            if (transform.position.y < -7f)
            {
                Destroy(gameObject);
            }
        }
    }
}
