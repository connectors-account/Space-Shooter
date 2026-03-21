using UnityEngine;

/// <summary>
/// Temporarily increases player movement speed.
/// </summary>
public class SpeedBoostPowerUp : PowerUpBase
{
    public float duration = 5f;
    public float speedMultiplier = 1.5f;

    protected override void ApplyPowerUp(PlayerController player)
    {
        player.ActivateSpeedBoost(duration, speedMultiplier);
    }
}
