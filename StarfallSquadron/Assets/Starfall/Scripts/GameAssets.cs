using UnityEngine;

namespace Starfall
{
    public sealed class GameAssets : ScriptableObject
    {
        public SpriteRenderer player;
        public SpriteRenderer[] enemies;
        public SpriteRenderer bolt;
        public SpriteRenderer[] pickups;
        public Sprite particle;
        public AudioClip fire, enemy, hit, explosion, pickup, wave, win, lose, ui, ambient;
    }
}
