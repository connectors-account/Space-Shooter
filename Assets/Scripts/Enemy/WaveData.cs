using UnityEngine;

namespace SpaceShooter.Enemy
{
    /// <summary>Enemy archetypes that a wave can spawn.</summary>
    public enum EnemyType
    {
        Drone,
        Fighter,
        Bomber,
        Boss
    }

    /// <summary>
    /// One entry in a wave: which enemy type and how many to spawn.
    /// </summary>
    [System.Serializable]
    public class WaveEntry
    {
        public EnemyType enemyType = EnemyType.Drone;
        public int count = 5;
    }

    /// <summary>
    /// ScriptableObject describing a single wave. Create assets via
    /// Assets ▸ Create ▸ Space Shooter ▸ Wave Data.
    /// </summary>
    [CreateAssetMenu(fileName = "WaveData", menuName = "Space Shooter/Wave Data", order = 0)]
    public class WaveData : ScriptableObject
    {
        [Tooltip("Enemy groups spawned during this wave.")]
        public WaveEntry[] entries = new WaveEntry[]
        {
            new WaveEntry { enemyType = EnemyType.Drone, count = 6 }
        };

        [Tooltip("Seconds between individual enemy spawns.")]
        public float spawnInterval = 0.8f;

        [Tooltip("If true, this wave is a boss wave.")]
        public bool hasBoss = false;

        [Tooltip("Extra difficulty multiplier applied on top of the global scaling.")]
        public float difficultyMultiplier = 1f;
    }
}
