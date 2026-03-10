using UnityEngine;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    public class ShieldPowerUp : PowerUpBase
    {
        [Header("Shield Settings")]
        [SerializeField] private int shieldHits = 3;

        protected override void Start()
        {
            powerUpType = PowerUpType.Shield;
            base.Start();
        }

        protected override void ApplyEffect(PlayerController player)
        {
            player.AddShield(shieldHits);
        }
    }
}
