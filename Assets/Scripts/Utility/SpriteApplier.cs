// ============================================================
//  SpriteApplier.cs  –  Applies SpriteFactory sprites at runtime
//  Attach to each prefab root; choose the type from the enum.
//  This removes the need for any external texture files.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteApplier : MonoBehaviour
{
    public enum SpriteType
    {
        PlayerShip,
        BasicEnemy,
        FastEnemy,
        HeavyEnemy,
        Boss,
        PlayerBullet,
        EnemyBullet,
        PowerUp,
        Shield
    }

    [Header("Which sprite to apply")]
    public SpriteType spriteType = SpriteType.PlayerShip;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = spriteType switch
        {
            SpriteType.PlayerShip   => SpriteFactory.CreatePlayerShip(),
            SpriteType.BasicEnemy   => SpriteFactory.CreateBasicEnemy(),
            SpriteType.FastEnemy    => SpriteFactory.CreateFastEnemy(),
            SpriteType.HeavyEnemy   => SpriteFactory.CreateHeavyEnemy(),
            SpriteType.Boss         => SpriteFactory.CreateBoss(),
            SpriteType.PlayerBullet => SpriteFactory.CreatePlayerBullet(),
            SpriteType.EnemyBullet  => SpriteFactory.CreateEnemyBullet(),
            SpriteType.PowerUp      => SpriteFactory.CreatePowerUpSprite(),
            SpriteType.Shield       => SpriteFactory.CreateShieldSprite(),
            _                       => SpriteFactory.CreatePlayerShip()
        };
    }
}
