using UnityEngine;

namespace Starfall
{
    // Persisted by the editor builder, loaded once at runtime. No AssetDatabase in players.
    public sealed class GameAssets : ScriptableObject
    {
        public PlayerShip player;
        public EnemyShip scout, fan, spinner;
        public Projectile playerBullet, enemyBullet;
        public Pickup repair, rapid, spread;
        public Sprite star, spark;
        public AudioClip shoot, enemyShoot, explosion, hurt, collect, wave;
    }
}
