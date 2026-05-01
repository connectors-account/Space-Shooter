using System;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Centralized gameplay tuning values. Edit these in GameBootstrapper to rebalance the game.
    /// </summary>
    [Serializable]
    public class GameConfig
    {
        public float PlayAreaHalfWidth = 8.5f;
        public float PlayAreaHalfHeight = 4.8f;

        [Header("Player")]
        public int PlayerMaxHealth = 100;
        public float PlayerMoveSpeed = 7f;
        public float PlayerFireInterval = 0.18f;
        public float PlayerBulletSpeed = 12f;
        public int PlayerBulletDamage = 20;
        public int MaxWeaponLevel = 4;
        public float ShieldDuration = 7f;

        [Header("Enemy")]
        public float EnemyBaseMoveSpeed = 2f;
        public float EnemyBulletSpeed = 6.5f;
        public int EnemyBulletDamage = 12;

        [Header("Waves")]
        public int BaseEnemiesPerWave = 6;
        public int ExtraEnemiesPerWave = 2;
        public float SpawnInterval = 0.65f;
        public float WaveBreakDuration = 2f;
        public float DifficultyRampPerWave = 0.12f;

        [Header("Power Ups")]
        [Range(0f, 1f)] public float PowerUpDropChance = 0.24f;
        public float PowerUpFallSpeed = 2f;
        public int HealthPowerUpAmount = 25;

        [Header("Background")]
        public float BackgroundScrollSpeed = 0.75f;
        public float MidgroundScrollSpeed = 1.15f;
        public float ForegroundScrollSpeed = 1.6f;
    }
}
