using System.Collections;
using UnityEngine;

namespace Starfall
{
    public enum GameState { Title, Playing, Paused, GameOver }
    public enum EnemyKind { Scout, Gunship, Commander }
    public enum PickupKind { Repair, RapidFire, Shield }

    public sealed class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }
        public PlayerShip playerPrefab;
        public EnemyShip[] enemyPrefabs;
        public Projectile playerBulletPrefab;
        public Projectile enemyBulletPrefab;
        public Pickup[] pickupPrefabs;
        public GameAudio audioSystem;
        public Transform actors;
        public GameState State { get; private set; } = GameState.Title;
        public PlayerShip Player { get; private set; }
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int Wave { get; private set; }
        public string Announcement { get; private set; } = "";
        public float HalfWidth => Camera.main.orthographicSize * Camera.main.aspect;
        public bool IsPlaying => State == GameState.Playing;
        private Coroutine waveRoutine;
        private int livingEnemies;
        private int kills;
        private int drops;
        private float announcementUntil;

        private void Awake()
        {
            Instance = this;
            Time.timeScale = 1f;
            Application.targetFrameRate = 120;
            Best = PlayerPrefs.GetInt("Starfall.Best", 0);
            // Only meaningful pairs interact. Swept projectile queries use the same target layers.
            for (int a = 8; a <= 12; a++)
                for (int b = a; b <= 12; b++)
                    Physics2D.IgnoreLayerCollision(a, b, !((a == 8 && (b == 9 || b == 11 || b == 12)) || (a == 9 && b == 10)));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (State == GameState.Playing) Pause();
                else if (State == GameState.Paused) Resume();
            }
            if (Input.GetKeyDown(KeyCode.Return) && (State == GameState.Title || State == GameState.GameOver)) StartGame();
            if (Input.GetKeyDown(KeyCode.R) && State == GameState.GameOver) StartGame();
            if (Time.time > announcementUntil) Announcement = "";
        }

        public void StartGame()
        {
            SaveBest(); // Preserve the current run when restarting from the pause menu.
            if (waveRoutine != null) StopCoroutine(waveRoutine);
            ClearActors();
            Time.timeScale = 1f;
            audioSystem.StopEffects();
            Score = Wave = kills = drops = livingEnemies = 0;
            State = GameState.Playing;
            Player = Instantiate(playerPrefab, new Vector3(0, -6.3f, 0), Quaternion.identity, actors);
            waveRoutine = StartCoroutine(Waves());
        }

        private IEnumerator Waves()
        {
            while (State == GameState.Playing || State == GameState.Paused)
            {
                Wave++;
                Announce("WAVE " + Wave, 2f);
                yield return new WaitForSeconds(1.8f);
                int count = 4 + Mathf.Min(Wave * 2, 20);
                for (int i = 0; i < count; i++)
                {
                    int kind = Wave >= 3 && i % 6 == 5 ? 2 : Wave >= 2 && i % 3 == 2 ? 1 : 0;
                    float x = Random.Range(-HalfWidth + 1.2f, HalfWidth - 1.2f);
                    EnemyShip enemy = Instantiate(enemyPrefabs[kind], new Vector3(x, 10.1f, 0), Quaternion.identity, actors);
                    enemy.Configure(Wave, i);
                    livingEnemies++;
                    yield return new WaitForSeconds(Mathf.Max(0.38f, 1.05f - Wave * 0.045f));
                }
                while (livingEnemies > 0) yield return null;
                foreach (Projectile shot in actors.GetComponentsInChildren<Projectile>())
                    if (!shot.friendly) Destroy(shot.gameObject);
                Score += 100 * Wave;
                Announce("SECTOR CLEAR  +" + (100 * Wave), 2f);
                yield return new WaitForSeconds(2.5f);
            }
        }

        public void EnemyRemoved(EnemyShip enemy, bool destroyed, bool escapePenalty = true)
        {
            livingEnemies = Mathf.Max(0, livingEnemies - 1);
            if (!IsPlaying) return;
            if (!destroyed)
            {
                if (escapePenalty && Player != null) Player.Damage(1);
                return;
            }
            Score += enemy.points;
            kills++;
            audioSystem.Play(SoundCue.Explosion);
            if (kills % 3 == 0 || Random.value < 0.12f)
            {
                // Cycling guarantees all three functional pickups appear, not just random luck.
                int index = drops++ % pickupPrefabs.Length;
                Instantiate(pickupPrefabs[index], enemy.transform.position, Quaternion.identity, actors);
            }
        }

        public void Fire(bool friendly, Vector2 origin, Vector2 direction, float speed)
        {
            Projectile prefab = friendly ? playerBulletPrefab : enemyBulletPrefab;
            Projectile bullet = Instantiate(prefab, origin, Quaternion.identity, actors);
            bullet.Launch(direction, speed);
        }

        public void Announce(string text, float seconds = 1.5f)
        {
            Announcement = text;
            announcementUntil = Time.time + seconds;
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            State = GameState.Paused;
            Time.timeScale = 0f;
            audioSystem.StopEffects();
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            State = GameState.Playing;
            Time.timeScale = 1f;
        }

        public void EndGame()
        {
            if (!IsPlaying) return;
            State = GameState.GameOver;
            Time.timeScale = 0f;
            if (waveRoutine != null) StopCoroutine(waveRoutine);
            SaveBest();
            audioSystem.Play(SoundCue.GameOver);
        }

        public void ShowTitle()
        {
            SaveBest();
            if (waveRoutine != null) StopCoroutine(waveRoutine);
            State = GameState.Title;
            Time.timeScale = 1f;
            ClearActors();
            audioSystem.StopEffects();
            Announcement = "";
        }

        private void SaveBest()
        {
            if (Score <= Best) return;
            Best = Score;
            PlayerPrefs.SetInt("Starfall.Best", Best);
            PlayerPrefs.Save();
        }

        private void ClearActors()
        {
            for (int i = actors.childCount - 1; i >= 0; i--)
            {
                GameObject child = actors.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            Player = null;
        }

        public void Quit()
        {
            SaveBest();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!focus && IsPlaying) Pause();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (Instance == this) Instance = null;
        }
    }
}
