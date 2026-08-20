using UnityEngine;
using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>
    /// Grants a shield that absorbs one hit. This is a one-shot effect — it persists on the
    /// player until the next hit consumes it, so it is not time-based.
    /// </summary>
    public class ShieldPowerUp : PowerUpBase
    {
        protected override bool IsTimed => false;

        protected override Sprite CreateSprite()
        {
            return SpriteGenerator.CreateCircle(14, new Color(0.3f, 0.6f, 1f));
        }

        public override void Apply(PlayerShooter shooter, PlayerHealth health)
        {
            if (health != null) health.SetShield(true);
        }

        public override void Remove()
        {
            // Shield removal is handled by PlayerHealth when the hit is absorbed.
        }
    }
}
