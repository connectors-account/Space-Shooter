using UnityEngine;

namespace SpaceShooter.Data
{
    /// <summary>
    /// Data-driven definition of an enemy type. Create assets via
    /// Assets > Create > Space Shooter > Enemy Data.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Space Shooter/Enemy Data", order = 1)]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName = "Grunt";

        [Header("Stats")]
        public int health = 30;
        public int scoreValue = 100;
        public float moveSpeed = 3f;

        [Header("Combat")]
        [Tooltip("Seconds between shots. 0 or less means this enemy does not shoot.")]
        public float shootInterval = 1.5f;
        public int bulletDamage = 10;

        [Header("Drops")]
        [Range(0f, 1f)] public float powerUpDropChance = 0.1f;

        [Header("Visuals")]
        public Sprite sprite;
        public GameObject explosionPrefab;

        [Tooltip("Optional tint applied to the enemy sprite renderer.")]
        public Color tint = Color.white;
    }
}
