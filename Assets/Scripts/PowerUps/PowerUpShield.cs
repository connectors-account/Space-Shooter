using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>Grants a temporary shield that absorbs the next hit.</summary>
    public class PowerUpShield : PowerUpBase
    {
        public override PowerUpType Type => PowerUpType.Shield;
        protected override string PoolKey => Constants.PoolPowerUpShield;

        public override void Apply(PlayerController controller, PlayerHealth health, PlayerShooter shooter)
        {
            if (health != null)
                health.ActivateShield(duration);
        }
    }
}
