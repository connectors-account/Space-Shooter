using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>Temporarily increases the player's rate of fire.</summary>
    public class PowerUpRapidFire : PowerUpBase
    {
        public override PowerUpType Type => PowerUpType.RapidFire;
        protected override string PoolKey => Constants.PoolPowerUpRapidFire;

        public override void Apply(PlayerController controller, PlayerHealth health, PlayerShooter shooter)
        {
            if (shooter != null)
                shooter.ActivateRapidFire(duration);
        }
    }
}
