using System.Collections;
using UnityEngine;

namespace OrbitBreaker
{
    public sealed class WaveDirector : MonoBehaviour
    {
        private GameController game;
        private System.Random random;

        public void Begin(GameController controller)
        {
            StopAllCoroutines();
            game = controller;
            random = new System.Random(2026);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (game.Phase == GamePhase.Playing || game.Phase == GamePhase.Paused)
            {
                game.Wave++;
                game.ui.Announce("WAVE " + game.Wave);
                game.Play(game.assets.wave, .5f);
                // A safe start and a quiet interval before each new wave.
                yield return new WaitForSeconds(2f);
                int count = Mathf.Min(22, 5 + game.Wave * 2);
                float interval = Mathf.Max(.42f, 1.05f - game.Wave * .04f);
                for (int i = 0; i < count; i++)
                {
                    if (game.Phase == GamePhase.GameOver || game.Phase == GamePhase.Title) yield break;
                    // Wave 1 teaches aimed shots, wave 2 adds fans, wave 3 adds rotating radial bursts.
                    int types = Mathf.Min(game.Wave, game.assets.enemies.Length);
                    int type = random.Next(types);
                    float x = (float)(random.NextDouble() * 14f - 7f);
                    Enemy enemy = Instantiate(game.assets.enemies[type], new Vector3(x, 5.65f, 0f),
                        Quaternion.Euler(0, 0, 180f), game.World);
                    enemy.Initialize(game, game.Wave, (float)random.NextDouble() * 6.28f);
                    yield return new WaitForSeconds(interval);
                }
                while (game.EnemyCount > 0) yield return null;
                game.ui.Announce("SECTOR CLEAR");
                yield return new WaitForSeconds(2f);
            }
        }
    }
}
