/// <summary>
/// Grants the player a shield that absorbs one hit.
/// </summary>
public class ShieldPowerUp : PowerUpBase
{
    protected override void ApplyPowerUp(PlayerController player)
    {
        player.ActivateShield();
    }
}
