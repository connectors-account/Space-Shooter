using UnityEngine;
using SpaceShooter.Player;
using SpaceShooter.Utilities;

namespace SpaceShooter.PowerUps
{
    /// <summary>Enables triple-shot firing for the power-up duration (10s default).</summary>
    public class TripleShotPowerUp : PowerUpBase
    {
        private PlayerShooter _shooter;

        protected override Sprite CreateSprite()
        {
            return SpriteGenerator.CreateCircle(14, new Color(0.4f, 1f, 0.4f));
        }

        public override void Apply(PlayerShooter shooter, PlayerHealth health)
        {
            _shooter = shooter;
            if (_shooter != null) _shooter.SetTripleShot(true);
        }

        public override void Remove()
        {
            if (_shooter != null) _shooter.SetTripleShot(false);
        }
    }
}
