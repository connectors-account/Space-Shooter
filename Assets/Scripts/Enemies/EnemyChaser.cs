using UnityEngine;

namespace SpaceShooter.Enemies
{
    public class EnemyChaser : EnemyBase
    {
        [SerializeField] private float horizontalTrackStrength = 1.2f;
        private Transform player;

        protected override void Awake()
        {
            base.Awake();
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        protected override Vector2 GetMovementDirection()
        {
            if (player == null)
            {
                return Vector2.down;
            }

            float horizontalDelta = Mathf.Clamp(player.position.x - transform.position.x, -1f, 1f);
            return new Vector2(horizontalDelta * horizontalTrackStrength, -1f);
        }
    }
}
