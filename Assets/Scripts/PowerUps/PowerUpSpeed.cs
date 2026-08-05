using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>Temporarily boosts the player's movement speed.</summary>
    public class PowerUpSpeed : PowerUpBase
    {
        [UnityEngine.SerializeField] private float speedMultiplier = 1.6f;

        public override PowerUpType Type => PowerUpType.Speed;
        protected override string PoolKey => Constants.PoolPowerUpSpeed;

        public override void Apply(PlayerController controller, PlayerHealth health, PlayerShooter shooter)
        {
            if (controller != null)
                controller.ActivateSpeedBoost(speedMultiplier, duration);
        }
    }
}
