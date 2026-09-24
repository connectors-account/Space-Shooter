using UnityEngine;

namespace Starward
{
    public enum GamePhase { Menu, Playing, Paused, GameOver }

    public sealed class GameManager : MonoBehaviour
    {
        public PlayerController playerPrefab;
        public Enemy enemyPrefab;
        public Bullet playerBulletPrefab, enemyBulletPrefab;
        public Pickup repairPrefab, weaponPrefab;
        public Burst burstPrefab;
        public AudioBank audioBank;
        public WaveDirector director;
        public Transform entities;
        public GamePhase Phase { get; private set; }
        public PlayerController Player { get; private set; }
        public int Score { get; private set; }
        public int Best { get; private set; }
        public int Wave { get; private set; }
        public bool IsPlaying { get { return Phase == GamePhase.Playing; } }
        int kills;
        const string BestKey = "Starward.Best";

        void Awake()
        {
            Time.timeScale = 1;
            Application.targetFrameRate = 60;
            Best = PlayerPrefs.GetInt(BestKey, 0);
            Phase = GamePhase.Menu;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) audioBank.ToggleMute();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (IsPlaying) Pause();
                else if (Phase == GamePhase.Paused) Resume();
            }
            if (Input.GetKeyDown(KeyCode.Return) && (Phase == GamePhase.Menu || Phase == GamePhase.GameOver)) StartRun();
            if (Input.GetKeyDown(KeyCode.R) && Phase == GamePhase.GameOver) StartRun();
        }

        public void StartRun()
        {
            ClearRun();
            Score = kills = 0;
            Wave = 0;
            Time.timeScale = 1;
            Phase = GamePhase.Playing;
            Player = Instantiate(playerPrefab, new Vector3(0, -3.8f, 0), Quaternion.identity, entities);
            Player.Initialize(this);
            director.Begin(this);
            audioBank.Play(SoundId.UI);
        }

        void ClearRun()
        {
            director.Stop();
            // Deactivate immediately: Destroy is deferred until the end of the frame.
            for (int i = entities.childCount - 1; i >= 0; i--)
            {
                GameObject child = entities.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
            Player = null;
        }

        public void NewWave(int wave)
        {
            Wave = wave;
            audioBank.Play(SoundId.Wave);
        }
        public void Pause()
        {
            if (!IsPlaying) return;
            Phase = GamePhase.Paused;
            Time.timeScale = 0;
        }
        public void Resume()
        {
            if (Phase != GamePhase.Paused) return;
            Phase = GamePhase.Playing;
            Time.timeScale = 1;
        }
        public void EndRun()
        {
            if (!IsPlaying) return;
            SaveBest();
            Phase = GamePhase.GameOver;
            Time.timeScale = 0;
            director.Stop();
            audioBank.Play(SoundId.GameOver);
        }
        public void MainMenu()
        {
            SaveBest();
            ClearRun();
            Phase = GamePhase.Menu;
            Time.timeScale = 1;
        }
        public void EnemyRemoved(Vector3 position, bool defeated)
        {
            director.EnemyRemoved();
            if (!defeated || !IsPlaying) return;
            Score += 100 + Wave * 10;
            if (Score > Best) Best = Score;
            kills++;
            SpawnBurst(position, new Color(1f, .35f, .22f));
            audioBank.Play(SoundId.Explosion);
            // Deterministic alternating drops: first upgrade on kill 4, repair on kill 8.
            if (kills % 4 == 0)
            {
                Pickup template = kills % 8 == 0 ? repairPrefab : weaponPrefab;
                Pickup item = Instantiate(template, position, Quaternion.identity, entities);
                item.Initialize(this);
            }
        }
        public void SpawnBurst(Vector3 position, Color color)
        {
            Burst effect = Instantiate(burstPrefab, position, Quaternion.identity, entities);
            effect.Initialize(color);
        }
        public void Fire(bool friendly, Vector3 origin, Vector2 direction)
        {
            Bullet shot = Instantiate(friendly ? playerBulletPrefab : enemyBulletPrefab, origin, Quaternion.identity, entities);
            shot.Initialize(this, direction, friendly);
        }
        void SaveBest()
        {
            if (Score > Best) Best = Score;
            PlayerPrefs.SetInt(BestKey, Best);
            PlayerPrefs.Save();
        }
        void OnApplicationFocus(bool focused) { if (!focused) Pause(); }
        void OnApplicationPause(bool paused) { if (paused) Pause(); }
        void OnApplicationQuit() { SaveBest(); }
        void OnDestroy() { Time.timeScale = 1; }
        public void Quit()
        {
            SaveBest();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
