using UnityEngine;
using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>Doubles the player's fire rate for the power-up duration (10s default).</summary>
    public class RapidFirePowerUp : PowerUpBase
    {
        private PlayerShooter _shooter;

        protected override Sprite CreateSprite()
        {
            return SpriteGenerator.CreateCircle(14, new Color(1f, 0.85f, 0.2f));
        }

        public override void Apply(PlayerShooter shooter, PlayerHealth health)
        {
            _shooter = shooter;
            if (_shooter != null) _shooter.SetRapidFire(true);
        }

        public override void Remove()
        {
            if (_shooter != null) _shooter.SetRapidFire(false);
        }
    }
}
