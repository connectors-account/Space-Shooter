using UnityEngine;

namespace SpaceShooter.Utilities
{
    /// <summary>
    /// Central location for every string constant used across the project:
    /// tags, layer names, scene names, object-pool keys and audio-clip names.
    /// Keeping them here prevents typos and keeps references in one place.
    /// </summary>
    public static class Constants
    {
        // ---------------------------------------------------------------
        // Tags
        // ---------------------------------------------------------------
        public const string TagPlayer       = "Player";
        public const string TagEnemy        = "Enemy";
        public const string TagPlayerBullet = "PlayerBullet";
        public const string TagEnemyBullet  = "EnemyBullet";
        public const string TagPowerUp      = "PowerUp";
        public const string TagBoss         = "Boss";

        // ---------------------------------------------------------------
        // Layer names (must match the layers created in Project Settings)
        // ---------------------------------------------------------------
        public const string LayerDefault      = "Default";
        public const string LayerPlayer       = "Player";
        public const string LayerEnemy        = "Enemy";
        public const string LayerPlayerBullet = "PlayerBullet";
        public const string LayerEnemyBullet  = "EnemyBullet";
        public const string LayerPowerUp      = "PowerUp";

        // ---------------------------------------------------------------
        // Scene names (must match entries in Build Settings)
        // ---------------------------------------------------------------
        public const string SceneMainMenu = "MainMenu";
        public const string SceneGame     = "Game";

        // ---------------------------------------------------------------
        // Object-pool keys
        // ---------------------------------------------------------------
        public const string PoolPlayerBullet = "PlayerBullet";
        public const string PoolEnemyBullet  = "EnemyBullet";
        public const string PoolEnemyDrone   = "EnemyDrone";
        public const string PoolEnemyFighter = "EnemyFighter";
        public const string PoolEnemyBomber  = "EnemyBomber";
        public const string PoolEnemyBoss    = "EnemyBoss";
        public const string PoolExplosion    = "Explosion";
        public const string PoolPowerUpShield     = "PowerUpShield";
        public const string PoolPowerUpRapidFire  = "PowerUpRapidFire";
        public const string PoolPowerUpTripleShot = "PowerUpTripleShot";
        public const string PoolPowerUpBomb       = "PowerUpBomb";
        public const string PoolPowerUpSpeed      = "PowerUpSpeed";

        // ---------------------------------------------------------------
        // Audio-clip names (files placed under Resources/Audio)
        // ---------------------------------------------------------------
        public const string SfxPlayerShoot  = "sfx_player_shoot";
        public const string SfxEnemyShoot   = "sfx_enemy_shoot";
        public const string SfxExplosion    = "sfx_explosion";
        public const string SfxPlayerHit    = "sfx_player_hit";
        public const string SfxPowerUp      = "sfx_powerup";
        public const string SfxBomb         = "sfx_bomb";
        public const string SfxShieldUp     = "sfx_shield_up";
        public const string SfxShieldDown   = "sfx_shield_down";
        public const string SfxUiClick      = "sfx_ui_click";
        public const string SfxWaveStart    = "sfx_wave_start";
        public const string SfxBossSpawn    = "sfx_boss_spawn";

        public const string MusicMenu    = "music_menu";
        public const string MusicGame    = "music_game";
        public const string MusicBoss    = "music_boss";

        // ---------------------------------------------------------------
        // PlayerPrefs keys
        // ---------------------------------------------------------------
        public const string PrefsHighScore   = "sc_high_score";
        public const string PrefsHighScores  = "sc_high_scores_json";
        public const string PrefsSfxVolume   = "sc_sfx_volume";
        public const string PrefsMusicVolume = "sc_music_volume";

        // ---------------------------------------------------------------
        // Gameplay tuning constants
        // ---------------------------------------------------------------
        public const float MultiplierResetTime = 5f;   // seconds without a kill
        public const int   MaxHighScoreEntries  = 5;
    }

    /// <summary>Types of power-up available in the game.</summary>
    public enum PowerUpType
    {
        Shield,
        RapidFire,
        TripleShot,
        Bomb,
        Speed
    }
}
