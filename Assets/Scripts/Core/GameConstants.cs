using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Central static store of all tuning values, camera bounds, layer names and tags.
    /// Nothing here changes at runtime; it is the single source of truth for balance.
    /// </summary>
    public static class GameConstants
    {
        #region Camera Bounds
        public const float CAMERA_LEFT = -9f;
        public const float CAMERA_RIGHT = 9f;
        public const float CAMERA_TOP = 5.5f;
        public const float CAMERA_BOTTOM = -5.5f;
        public const float ORTHOGRAPHIC_SIZE = 5.5f;
        #endregion

        #region Layer Names
        public const string LAYER_DEFAULT = "Default";
        public const string LAYER_PLAYER = "Player";
        public const string LAYER_ENEMY = "Enemy";
        public const string LAYER_PLAYER_BULLET = "PlayerBullet";
        public const string LAYER_ENEMY_BULLET = "EnemyBullet";
        public const string LAYER_POWERUP = "PowerUp";

        public const int LAYER_ID_DEFAULT = 0;
        public const int LAYER_ID_PLAYER = 6;
        public const int LAYER_ID_ENEMY = 7;
        public const int LAYER_ID_PLAYER_BULLET = 8;
        public const int LAYER_ID_ENEMY_BULLET = 9;
        public const int LAYER_ID_POWERUP = 10;
        #endregion

        #region Tags
        public const string TAG_PLAYER = "Player";
        public const string TAG_ENEMY = "Enemy";
        public const string TAG_BOSS = "Boss";
        public const string TAG_BULLET = "Bullet";
        public const string TAG_POWERUP = "PowerUp";
        #endregion

        #region Player Tuning
        public const float PLAYER_MOVE_SPEED = 6f;
        public const float PLAYER_BOOSTED_MOVE_SPEED = 10f;
        public const float PLAYER_SPEED_BOOST_DURATION = 6f;
        public const int PLAYER_MAX_HEALTH = 3;
        public const int PLAYER_MAX_SHIELD = 3;
        public const float PLAYER_INVINCIBILITY_TIME = 1.5f;
        public const float PLAYER_TILT_MAX_ANGLE = 25f;
        public const float PLAYER_START_Y = -4f;
        #endregion

        #region Bullet Tuning
        public const float PLAYER_BULLET_SPEED = 12f;
        public const float ENEMY_BULLET_SPEED = 6f;
        public const int PLAYER_BULLET_DAMAGE = 1;
        public const int ENEMY_BULLET_DAMAGE = 1;
        public const float BULLET_LIFETIME = 4f;
        public const int PLAYER_BULLET_POOL_SIZE = 30;
        public const int ENEMY_BULLET_POOL_SIZE = 50;
        #endregion

        #region Weapon Cooldowns
        public const float COOLDOWN_SINGLE = 0.2f;
        public const float COOLDOWN_DOUBLE = 0.25f;
        public const float COOLDOWN_TRIPLE = 0.25f;
        public const float COOLDOWN_SPREAD5 = 0.3f;
        public const float COOLDOWN_LASER_TICK = 0.05f;
        public const float LASER_DURATION = 2f;
        public const int LASER_DAMAGE_PER_TICK = 2;
        public const float POWERUP_TIMED_DURATION = 8f;
        #endregion

        #region Enemy Tuning
        public const float ENEMY_DIVER_SPEED = 3f;
        public const float ENEMY_DIVER_DIVE_MULTIPLIER = 1.5f;
        public const int ENEMY_DIVER_HEALTH = 2;
        public const int ENEMY_DIVER_SCORE = 100;
        public const float ENEMY_DIVER_FIRE_RATE = 2f;

        public const float ENEMY_FORMATION_SPEED = 2.5f;
        public const int ENEMY_FORMATION_HEALTH = 3;
        public const int ENEMY_FORMATION_SCORE = 150;
        public const float ENEMY_FORMATION_FIRE_RATE = 1.5f;
        public const float ENEMY_FORMATION_OSCILLATE_AMP = 1.5f;
        public const float ENEMY_FORMATION_OSCILLATE_FREQ = 1f;

        public const float ENEMY_CIRCLER_SPEED = 2f;
        public const int ENEMY_CIRCLER_HEALTH = 4;
        public const int ENEMY_CIRCLER_SCORE = 200;
        public const float ENEMY_CIRCLER_FIRE_RATE = 3f;
        public const float ENEMY_CIRCLER_ORBIT_RADIUS = 2f;
        public const float ENEMY_CIRCLER_ORBIT_SPEED = 1.5f;
        #endregion

        #region Boss Tuning
        public const int BOSS_MAX_HEALTH = 500;
        public const int BOSS_SCORE = 5000;
        public const float BOSS_MOVE_SPEED = 2f;
        public const float BOSS_PHASE2_SPEED_MULTIPLIER = 1.75f;
        public const float BOSS_SPREAD_FIRE_RATE = 1.5f;
        public const float BOSS_CIRCLE_FIRE_RATE = 4f;
        public const float BOSS_SPIRAL_FIRE_RATE = 0.15f;
        public const float BOSS_Y_POSITION = 3.5f;
        #endregion

        #region PowerUp Tuning
        public const float POWERUP_DRIFT_SPEED = 2f;
        public const float POWERUP_ROTATE_SPEED = 60f;
        public const float POWERUP_DROP_CHANCE = 0.10f;
        public const int POWERUP_HEALTHPACK_AMOUNT = 1;
        #endregion

        #region Wave Tuning
        public const float WAVE_ANNOUNCE_DELAY = 2f;
        public const float WAVE_ENEMY_SPAWN_GAP = 0.4f;
        public const float WAVE_COMPLETE_DELAY = 3f;
        public const float WAVE_DIFFICULTY_STEP = 0.15f;
        #endregion

        #region PlayerPrefs Keys
        public const string PREF_HIGH_SCORE = "HighScore";
        #endregion

        #region Audio
        public const int AUDIO_SAMPLE_RATE = 44100;
        public const int SFX_SOURCE_COUNT = 8;
        #endregion
    }
}
