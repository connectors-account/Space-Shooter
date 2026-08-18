using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Switches the player's weapon to Triple fire for 10 seconds.</summary>
    public class PowerUpTripleShot : PowerUpBase
    {
        private FireMode _previousMode = FireMode.Single;
        private bool _applied;

        public PowerUpTripleShot()
        {
            duration = 10f;
        }

        public override void Apply(GameObject player)
        {
            var ps = player.GetComponent<PlayerShooter>();
            if (ps == null) return;

            if (!_applied)
            {
                _previousMode = ps.fireMode;
                _applied = true;
            }
            ps.fireMode = FireMode.Triple;
        }

        public override void Expire(GameObject player)
        {
            var ps = player.GetComponent<PlayerShooter>();
            if (ps != null && _applied) ps.fireMode = _previousMode;
            _applied = false;
        }
    }
}
