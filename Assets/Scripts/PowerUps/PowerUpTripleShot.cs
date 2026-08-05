using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>Temporarily makes the player fire three bullets in a fan.</summary>
    public class PowerUpTripleShot : PowerUpBase
    {
        public override PowerUpType Type => PowerUpType.TripleShot;
        protected override string PoolKey => Constants.PoolPowerUpTripleShot;

        public override void Apply(PlayerController controller, PlayerHealth health, PlayerShooter shooter)
        {
            if (shooter != null)
                shooter.ActivateTripleShot(duration);
        }
    }
}
