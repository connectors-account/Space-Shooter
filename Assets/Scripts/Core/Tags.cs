/// <summary>
/// Central location for all string tags and layer names used across the project.
/// Avoids magic strings scattered throughout scripts.
/// </summary>
public static class Tags
{
    // Pool tags
    public const string PlayerBullet = "PlayerBullet";
    public const string EnemyBullet = "EnemyBullet";
    public const string EnemyBasic = "EnemyBasic";
    public const string EnemyFast = "EnemyFast";
    public const string EnemyTank = "EnemyTank";
    public const string EnemyBoss = "EnemyBoss";
    public const string Explosion = "Explosion";
    public const string PowerUp = "PowerUp";

    // Unity tags
    public const string Player = "Player";
    public const string Enemy = "Enemy";
    public const string Bullet = "Bullet";

    // Sorting layers
    public const string BackgroundLayer = "Background";
    public const string GameplayLayer = "Default";
    public const string UILayer = "UI";
}
