using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Data
{
    /// <summary>
    /// Spawn formations shared by the wave data and the enemy spawner.
    /// </summary>
    public enum Formation
    {
        Line,
        VShape,
        Random,
        Flanks
    }

    /// <summary>
    /// A single group of identical enemies within a wave.
    /// </summary>
    [Serializable]
    public class EnemySpawnEntry
    {
        public EnemyData enemyData;
        [Min(1)] public int count = 5;
        [Min(0.05f)] public float spawnInterval = 0.5f;
        public Formation formation = Formation.Line;
    }

    /// <summary>
    /// Data-driven definition of a wave. Create assets via
    /// Assets > Create > Space Shooter > Wave Data.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "Space Shooter/Wave Data", order = 0)]
    public class WaveData : ScriptableObject
    {
        [Header("Wave")]
        public int waveNumber = 1;

        [Header("Enemies")]
        public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();

        [Header("Boss")]
        public bool hasBoss = false;
        public EnemyData bossData;

        [Header("Timing")]
        [Tooltip("Delay before the next wave begins after this one is cleared.")]
        public float timeBetweenWaves = 3f;
    }
}
