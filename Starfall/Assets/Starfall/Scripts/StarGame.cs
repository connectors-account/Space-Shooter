using System.Collections;
using UnityEngine;

namespace Starfall
{
    public enum RunState { Menu, Playing, Paused, GameOver }

    [RequireComponent(typeof(AudioSource), typeof(GameHud), typeof(Starfield))]
    public sealed class StarGame : MonoBehaviour
    {
        public static StarGame Instance { get; private set; }
        public GameAssets assets;
        public RunState State { get; private set; }
        public PlayerShip Player { get; private set; }
        public Transform Actors { get; private set; }
        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int Best { get; private set; }
        public string Banner { get; private set; }
        public bool Muted { get; private set; }
        public const float HalfWidth = 8.88f, HalfHeight = 5f;
        AudioSource audioSource;
        Coroutine waveRoutine;
        float bannerUntil;
        int kills;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Time.timeScale = 1;
            assets = assets != null ? assets : Resources.Load<GameAssets>("GameAssets");
            if (assets == null) { Debug.LogError("Missing assets. Run Starfall > Rebuild Generated Content in Unity."); enabled = false; return; }
            Application.targetFrameRate = 120;
            Best = PlayerPrefs.GetInt("Starfall.Best", 0);
            Muted = PlayerPrefs.GetInt("Starfall.Muted", 0) != 0;
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
            Actors = new GameObject("Live actors").transform;
            Actors.SetParent(transform);
            State = RunState.Menu;
            GetComponent<Starfield>().Initialize(assets.star);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) ToggleMute();
            if (State == RunState.Playing && Input.GetKeyDown(KeyCode.Escape)) Pause();
            else if (State == RunState.Paused && Input.GetKeyDown(KeyCode.Escape)) Resume();
            else if ((State == RunState.Menu || State == RunState.GameOver) && Input.GetKeyDown(KeyCode.Return)) StartRun();
            if (State == RunState.GameOver && Input.GetKeyDown(KeyCode.R)) StartRun();
            if (Time.time > bannerUntil) Banner = "";
        }

        public void StartRun()
        {
            if (assets == null) return;
            ClearRun();
            Score = 0; Wave = 0; kills = 0;
            Time.timeScale = 1;
            State = RunState.Playing;
            Player = Instantiate(assets.player, new Vector3(0, -3.5f, 0), Quaternion.identity, Actors);
            waveRoutine = StartCoroutine(Waves());
        }

        void ClearRun()
        {
            if (waveRoutine != null) { StopCoroutine(waveRoutine); waveRoutine = null; }
            if (Actors != null)
                foreach (Transform child in Actors) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
            Player = null;
            Banner = "";
        }

        IEnumerator Waves()
        {
            while (State == RunState.Playing || State == RunState.Paused)
            {
                Wave++;
                Banner = "WAVE " + Wave.ToString("00"); bannerUntil = Time.time + 2.4f;
                Sound(assets.wave, 0.45f);
                yield return new WaitForSeconds(2);
                int count = GameRules.EnemiesInWave(Wave);
                for (int i = 0; i < count; i++)
                {
                    int type = GameRules.EnemyType(Wave, i);
                    EnemyShip prefab = type == 2 ? assets.spinner : type == 1 ? assets.fan : assets.scout;
                    var enemy = Instantiate(prefab, new Vector3(Random.Range(-7.3f, 7.3f), 5.8f, 0), Quaternion.identity, Actors);
                    enemy.Configure(Wave, i);
                    yield return new WaitForSeconds(Mathf.Max(0.38f, 1.1f - Wave * 0.045f));
                }
                while (Actors.GetComponentInChildren<EnemyShip>() != null) yield return new WaitForSeconds(0.25f);
                yield return new WaitForSeconds(1.2f);
            }
        }

        public void EnemyDestroyed(Vector3 position, int value)
        {
            if (State != RunState.Playing) return;
            Score += value; kills++;
            Burst(position, new Color(1, 0.48f, 0.28f), 16);
            Sound(assets.explosion, 0.6f);
            // A guaranteed drop every fifth kill avoids runs without power-ups.
            if (kills % 5 == 0)
            {
                int choice = (kills / 5 - 1) % 3;
                Pickup prefab = choice == 0 ? assets.spread : choice == 1 ? assets.repair : assets.rapid;
                Instantiate(prefab, position, Quaternion.identity, Actors);
            }
        }

        public void EndRun()
        {
            if (State != RunState.Playing) return;
            State = RunState.GameOver;
            if (Score > Best) { Best = Score; PlayerPrefs.SetInt("Starfall.Best", Best); PlayerPrefs.Save(); }
            Time.timeScale = 0;
        }
        public void Pause() { if (State == RunState.Playing) { State = RunState.Paused; Time.timeScale = 0; } }
        public void Resume() { if (State == RunState.Paused) { State = RunState.Playing; Time.timeScale = 1; } }
        public void Menu() { ClearRun(); Time.timeScale = 1; State = RunState.Menu; }
        void OnApplicationFocus(bool focus) { if (!focus) Pause(); }
        void OnDestroy() { if (Instance == this) { Instance = null; Time.timeScale = 1; } }
        public void ToggleMute()
        {
            Muted = !Muted;
            PlayerPrefs.SetInt("Starfall.Muted", Muted ? 1 : 0); PlayerPrefs.Save();
            if (Muted) audioSource.Stop();
        }
        public void Sound(AudioClip clip, float volume = 0.35f)
        {
            if (!Muted && clip != null && audioSource != null) audioSource.PlayOneShot(clip, volume);
        }
        public void Burst(Vector3 position, Color color, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Spark"); go.transform.SetParent(Actors); go.transform.position = position;
                var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = assets.spark; sr.color = color; sr.sortingOrder = 8;
                go.AddComponent<Spark>().Initialize(Random.insideUnitCircle * 3.5f, Random.Range(0.2f, 0.55f));
            }
        }
        public void Quit()
        {
            PlayerPrefs.Save();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
