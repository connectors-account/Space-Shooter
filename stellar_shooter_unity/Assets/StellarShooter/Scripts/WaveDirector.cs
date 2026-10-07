using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StellarShooter
{
    public sealed class WaveDirector : MonoBehaviour
    {
        public Enemy[] enemyPrefabs;
        private readonly List<Enemy> alive = new List<Enemy>();
        public void Begin(GameSession game) { StopWaves(); StartCoroutine(Run(game)); }
        public void StopWaves() { StopAllCoroutines(); alive.Clear(); }
        private IEnumerator Run(GameSession game)
        {
            // Fixed seed produces a repeatable run, without altering Unity's global random state.
            var random = new System.Random(4815);
            while (true)
            {
                game.Wave++;
                game.ui.Announce("WAVE " + game.Wave, 2);
                game.sound.Play(SoundCue.Wave);
                yield return new WaitForSeconds(2.5f);
                int count = GameRules.EnemyCount(game.Wave);
                for (int i = 0; i < count; i++)
                {
                    int available = Mathf.Min(game.Wave, enemyPrefabs.Length);
                    int kind = i % available;
                    float x = (float)(random.NextDouble() * 13 - 6.5);
                    Enemy enemy = Instantiate(enemyPrefabs[kind], new Vector3(x, 5.65f, 0), Quaternion.identity, game.actors);
                    enemy.Initialize(game, game.Wave);
                    alive.Add(enemy);
                    yield return new WaitForSeconds(Mathf.Max(.35f, .85f - game.Wave * .035f));
                }
                while (alive.Count > 0)
                {
                    alive.RemoveAll(enemy => enemy == null);
                    yield return null;
                }
                game.ui.Announce("SECTOR CLEAR", 1.6f);
                yield return new WaitForSeconds(2);
            }
        }
    }
}
