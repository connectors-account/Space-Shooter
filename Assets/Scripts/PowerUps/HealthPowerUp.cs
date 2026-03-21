/// <summary>
/// Restores a portion of the player's health.
/// </summary>
public class HealthPowerUp : PowerUpBase
{
    public int healAmount = 30;

    protected override void ApplyPowerUp(PlayerController player)
    {
        player.Health.Heal(healAmount);
    }
}
