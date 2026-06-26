namespace SpaceShooter.Core
{
    /// <summary>
    /// High level state of the overall game session.
    /// Driven by <see cref="SpaceShooter.Managers.GameManager"/>.
    /// </summary>
    public enum GameState
    {
        /// <summary>Player is in the main menu (handled in MainMenu scene).</summary>
        MainMenu,
        /// <summary>Active gameplay.</summary>
        Playing,
        /// <summary>Gameplay temporarily suspended (time scale 0).</summary>
        Paused,
        /// <summary>Player has lost all lives.</summary>
        GameOver,
        /// <summary>Player has cleared all configured waves.</summary>
        Victory
    }

    /// <summary>
    /// Identifies which "team" an object belongs to so collisions can be resolved generically.
    /// </summary>
    public enum Faction
    {
        Player,
        Enemy,
        Neutral
    }

    /// <summary>
    /// The available enemy archetypes. Each maps to a distinct movement / attack behaviour.
    /// </summary>
    public enum EnemyType
    {
        /// <summary>Moves straight down at a constant speed.</summary>
        Basic,
        /// <summary>Moves down while oscillating horizontally in a zig-zag.</summary>
        Zigzag,
        /// <summary>Moves down following a smooth sine / circular path.</summary>
        Circular,
        /// <summary>Powerful boss with multiple attack patterns. Spawns every 5th wave.</summary>
        Boss
    }

    /// <summary>
    /// Describes how a fired bullet travels and is rendered.
    /// </summary>
    public enum BulletPattern
    {
        /// <summary>Single bullet travelling in its initial direction.</summary>
        Straight,
        /// <summary>Bullet aimed at a target position at spawn time.</summary>
        Aimed,
        /// <summary>One of several bullets fired outward in a radial spray.</summary>
        Radial
    }

    /// <summary>
    /// The different power-up effects a player can collect.
    /// </summary>
    public enum PowerUpType
    {
        /// <summary>Restores a portion of player health.</summary>
        Health,
        /// <summary>Grants temporary invincibility.</summary>
        Shield,
        /// <summary>Increases fire rate for a limited time.</summary>
        RapidFire,
        /// <summary>Fires three bullets in a spread for a limited time.</summary>
        SpreadShot,
        /// <summary>Multiplies score gained for a limited time.</summary>
        ScoreMultiplier
    }
}
