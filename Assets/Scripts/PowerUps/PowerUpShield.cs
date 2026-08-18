using UnityEngine;

namespace SpaceShooter
{
    /// <summary>Grants the player a shield with 3 hit points. Instant, no expiry.</summary>
    public class PowerUpShield : PowerUpBase
    {
        [Tooltip("Shield hit points granted.")]
        public int shieldAmount = 3;

        public PowerUpShield()
        {
            duration = 0f;
        }

        public override void Apply(GameObject player)
        {
            var ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.ActivateShield(shieldAmount);
        }

        public override void Expire(GameObject player)
        {
            // Shield is consumed by damage, so nothing to undo here.
        }
    }
}
