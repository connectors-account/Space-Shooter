using System.Collections;
using UnityEngine;

namespace NovaGuard
{
    public sealed class WaveDirector : MonoBehaviour
    {
        public void Begin() { Stop(); StartCoroutine(Run()); }
        public void Stop() { StopAllCoroutines(); }

        private IEnumerator Run()
        {
            GameController game = GameController.Instance;
            yield return new WaitForSeconds(1.5f);
            int wave = 0;
            while (true)
            {
                wave++;
                game.SetWave(wave);
                int count = Mathf.Min(18, 4 + wave * 2);
                for (int i = 0; i < count; i++)
                {
                    int type = 0;
                    if (wave >= 2 && i % 3 == 1) type = 1;
                    if (wave >= 3 && i % 5 == 4) type = 2;
                    game.SpawnEnemy(type, new Vector2(Random.Range(-11.8f, 11.8f), 10));
                    yield return new WaitForSeconds(Mathf.Max(0.5f, 1.2f - wave * 0.06f));
                }
                while (game.EnemyCount > 0) yield return null;
                game.AddScore(wave * 100);
                game.ui.Announce("WAVE CLEAR / +" + wave * 100, 2f);
                // Guaranteed assistance complements random enemy drops.
                PowerUp bonus = game.powerUpPrefabs[(wave - 1) % game.powerUpPrefabs.Length];
                Instantiate(bonus, new Vector3(Random.Range(-7f, 7f), 3, 0), Quaternion.identity, game.worldRoot);
                yield return new WaitForSeconds(3.5f);
            }
        }
    }
}
