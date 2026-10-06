using UnityEngine;

namespace OrbitBreaker
{
    /// <summary>All runtime dependencies are serialized by the first-open Editor builder.</summary>
    public sealed class GameAssets : ScriptableObject
    {
        public PlayerController player;
        public Enemy[] enemies;
        public Bullet playerBullet;
        public Bullet enemyBullet;
        public Powerup[] powerups;
        public Sprite[] stars;
        public Sprite spark;
        public AudioClip shoot, enemyShot, hit, explode, pickup, wave, gameOver, ui;
    }
}
