using SpaceShooter.Player;
using SpaceShooter.Utilities;
using SpaceShooter.Weapons;

namespace SpaceShooter.PowerUps
{
    /// <summary>Adds one screen-clearing bomb to the player's stock.</summary>
    public class PowerUpBomb : PowerUpBase
    {
        public override PowerUpType Type => PowerUpType.Bomb;
        protected override string PoolKey => Constants.PoolPowerUpBomb;

        public override void Apply(PlayerController controller, PlayerHealth health, PlayerShooter shooter)
        {
            if (controller == null) return;
            var bomb = controller.GetComponent<Bomb>();
            if (bomb != null)
                bomb.AddBomb(1);
        }
    }
}
