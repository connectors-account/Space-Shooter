using UnityEngine;

namespace SpaceShooter.Core
{
    public static class GameLayers
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string PlayerBullet = "PlayerBullet";
        public const string EnemyBullet = "EnemyBullet";
        public const string PowerUp = "PowerUp";

        public static int GetLayerOrDefault(string layerName, int fallback = 0)
        {
            int value = LayerMask.NameToLayer(layerName);
            return value < 0 ? fallback : value;
        }
    }
}
