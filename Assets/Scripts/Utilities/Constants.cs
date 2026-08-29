namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Central location for all string and numeric constants used across the game.
    /// Keeping them here avoids "magic strings" scattered throughout the code base.
    /// </summary>
    public static class Constants
    {
        // ----------------------------------------------------------------
        // Tags (must match the tags configured in the Unity Tag Manager)
        // ----------------------------------------------------------------
        public static class Tags
        {
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string EnemyBullet = "EnemyBullet";
            public const string PlayerBullet = "PlayerBullet";
            public const string PowerUp = "PowerUp";
            public const string Ground = "Ground";
            public const string Boss = "Boss";
        }

        // ----------------------------------------------------------------
        // Layer names (must match the layers configured in the Layer Manager)
        // ----------------------------------------------------------------
        public static class Layers
        {
            public const string Default = "Default";
            public const string Player = "Player";
            public const string Enemy = "Enemy";
            public const string PlayerBullet = "PlayerBullet";
            public const string EnemyBullet = "EnemyBullet";
            public const string PowerUp = "PowerUp";
            public const string Background = "Background";
        }

        // ----------------------------------------------------------------
        // Scene names (must match the scenes added to Build Settings)
        // ----------------------------------------------------------------
        public static class Scenes
        {
            public const string MainMenu = "MainMenu";
            public const string Game = "Game";
            public const string GameOver = "GameOver";
        }

        // ----------------------------------------------------------------
        // Audio clip identifiers (informational / used for lookup tables)
        // ----------------------------------------------------------------
        public static class Audio
        {
            public const string Shoot = "shoot";
            public const string Explosion = "explosion";
            public const string PowerUp = "powerup";
            public const string PlayerHit = "playerhit";
            public const string BossAlert = "bossalert";
            public const string MenuMusic = "menumusic";
            public const string GameMusic = "gamemusic";
            public const string GameOverMusic = "gameovermusic";
        }

        // ----------------------------------------------------------------
        // PlayerPrefs keys
        // ----------------------------------------------------------------
        public static class PrefKeys
        {
            public const string HighScore = "HighScore";
            public const string MusicVolume = "MusicVolume";
            public const string SFXVolume = "SFXVolume";
        }

        // ----------------------------------------------------------------
        // Gameplay numeric constants
        // ----------------------------------------------------------------
        public const float PlayerMoveSpeed = 8f;
        public const int PlayerMaxHealth = 100;
        public const int PlayerStartLives = 3;

        public const float PlayerBulletSpeed = 15f;
        public const int PlayerBulletDamage = 10;
        public const float PlayerFireRate = 0.15f;

        public const float EnemyBulletSpeed = 8f;
        public const int EnemyBulletDamage = 10;

        public const float InvincibilityDuration = 1.5f;
        public const float BulletLifetime = 3f;

        public const int MinWeaponLevel = 1;
        public const int MaxWeaponLevel = 4;

        // Screen padding (in world units) used when clamping the player.
        public const float ScreenPadding = 0.5f;
    }
}
