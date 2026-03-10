using UnityEngine;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    public class HealthPack : PowerUpBase
    {
        [Header("Health Settings")]
        [SerializeField] private int healAmount = 30;

        protected override void Start()
        {
            powerUpType = PowerUpType.Health;
            base.Start();
        }

        protected override void ApplyEffect(PlayerController player)
        {
            player.Heal(healAmount);
        }
    }
}
