using UnityEngine;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    public class WeaponUpgrade : PowerUpBase
    {
        protected override void Start()
        {
            powerUpType = PowerUpType.WeaponUpgrade;
            base.Start();
        }

        protected override void ApplyEffect(PlayerController player)
        {
            player.UpgradeWeapon();
        }
    }
}
