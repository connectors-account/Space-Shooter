using UnityEngine;

/// <summary>
/// Grants the player triple-shot for a limited duration.
/// </summary>
public class MultiShotPowerUp : PowerUpBase
{
    public float duration = 5f;

    protected override void ApplyPowerUp(PlayerController player)
    {
        player.ActivateMultiShot(duration);
    }
}
