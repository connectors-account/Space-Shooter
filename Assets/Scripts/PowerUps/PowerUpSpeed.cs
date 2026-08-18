using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Temporarily increases the player's move speed by 4 for 8 seconds.</summary>
    public class PowerUpSpeed : PowerUpBase
    {
        [Tooltip("Amount added to the player's move speed.")]
        public float speedBonus = 4f;

        private bool _applied;

        public PowerUpSpeed()
        {
            duration = 8f;
        }

        public override void Apply(GameObject player)
        {
            if (_applied) return;
            var pc = player.GetComponent<PlayerController>();
            if (pc == null) return;

            pc.moveSpeed += speedBonus;
            _applied = true;
        }

        public override void Expire(GameObject player)
        {
            if (!_applied) return;
            var pc = player.GetComponent<PlayerController>();
            if (pc != null) pc.moveSpeed -= speedBonus;
            _applied = false;
        }
    }
}
