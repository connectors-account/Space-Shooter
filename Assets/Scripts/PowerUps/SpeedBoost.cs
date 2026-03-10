using UnityEngine;
using SpaceShooter.Player;

namespace SpaceShooter.PowerUps
{
    public class SpeedBoost : PowerUpBase
    {
        [Header("Speed Boost Settings")]
        [SerializeField] private float speedMultiplier = 1.5f;
        [SerializeField] private float duration = 5f;

        protected override void Start()
        {
            powerUpType = PowerUpType.SpeedBoost;
            base.Start();
        }

        protected override void ApplyEffect(PlayerController player)
        {
            player.IncreaseSpeed(speedMultiplier, duration);
        }
    }
}
