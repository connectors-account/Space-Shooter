using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>Enemy archetypes that a wave can spawn.</summary>
    public enum EnemyType { Diver, Formation, Circler, Boss }

    /// <summary>
    /// A single spawn instruction inside a wave: which enemy, how many,
    /// timing, position and whether the group should form up.
    /// </summary>
    [System.Serializable]
    public class EnemySpawnEntry
    {
        public EnemyType type = EnemyType.Diver;
        public int count = 1;
        public float spawnDelay = 0.4f;
        public Vector2 spawnPosition = Vector2.zero;
        public bool useFormation = false;

        public EnemySpawnEntry() { }

        public EnemySpawnEntry(EnemyType type, int count, float spawnDelay, Vector2 spawnPosition, bool useFormation)
        {
            this.type = type;
            this.count = count;
            this.spawnDelay = spawnDelay;
            this.spawnPosition = spawnPosition;
            this.useFormation = useFormation;
        }
    }

    /// <summary>
    /// Serializable definition of a single wave: its number, display name,
    /// whether it is a boss wave, and the list of spawn entries.
    /// </summary>
    [System.Serializable]
    public class WaveData
    {
        public int waveNumber = 1;
        public string waveName = "Wave";
        public bool isBossWave = false;
        public List<EnemySpawnEntry> enemies = new List<EnemySpawnEntry>();

        public WaveData() { }

        public WaveData(int waveNumber, string waveName, bool isBossWave, List<EnemySpawnEntry> enemies)
        {
            this.waveNumber = waveNumber;
            this.waveName = waveName;
            this.isBossWave = isBossWave;
            this.enemies = enemies ?? new List<EnemySpawnEntry>();
        }
    }
}
