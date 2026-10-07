using UnityEngine;

namespace StellarShooter
{
    public enum GamePhase { Menu, Playing, Paused, GameOver }

    public sealed class GameSession : MonoBehaviour
    {
        public PlayerController playerPrefab;
        public Transform actors;
        public WaveDirector waves;
        public GameUI ui;
        public SoundBank sound;
        public ImpactEffect impactPrefab;
        public Pickup[] pickupPrefabs;
        public GamePhase Phase { get; private set; }
        public PlayerController Player { get; private set; }
        public int Score { get; private set; }
        public int Wave { get; set; }
        public int Best { get; private set; }
        public bool IsPlaying => Phase == GamePhase.Playing;
        private int kills;

        private void Awake()
        {
            Time.timeScale = 1f;
            Application.targetFrameRate = 120;
            Best = PlayerPrefs.GetInt("StellarShooter.Best", 0);
            // Only intended pairs interact, regardless of pre-existing project settings.
            for (int a = 8; a <= 12; a++)
                for (int b = a; b <= 12; b++) Physics2D.IgnoreLayerCollision(a, b, true);
            Physics2D.IgnoreLayerCollision(8, 9, false);  // Player / enemy
            Physics2D.IgnoreLayerCollision(8, 11, false); // Player / hostile shot
            Physics2D.IgnoreLayerCollision(8, 12, false); // Player / pickup
            Physics2D.IgnoreLayerCollision(9, 10, false); // Enemy / friendly shot
        }

        private void Start() => SetPhase(GamePhase.Menu);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) sound.ToggleMute();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                if (IsPlaying) Pause();
                else if (Phase == GamePhase.Paused) Resume();
            }
            if (Phase == GamePhase.GameOver && Input.GetKeyDown(KeyCode.R)) StartGame();
        }

        public void StartGame()
        {
            waves.StopWaves();
            ClearActors();
            Score = 0;
            Wave = 0;
            kills = 0;
            Player = Instantiate(playerPrefab, new Vector3(0, -3.4f, 0), Quaternion.identity, actors);
            Player.Initialize(this);
            SetPhase(GamePhase.Playing);
            waves.Begin(this);
        }

        public void Pause()
        {
            if (!IsPlaying) return;
            Player.StopMovement();
            SetPhase(GamePhase.Paused);
        }
        public void Resume()
        {
            if (Phase == GamePhase.Paused) SetPhase(GamePhase.Playing);
        }
        public void MainMenu()
        {
            waves.StopWaves();
            ClearActors();
            SetPhase(GamePhase.Menu);
        }
        public void Quit()
        {
            Time.timeScale = 1;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        public void EndGame()
        {
            if (!IsPlaying) return;
            waves.StopWaves();
            if (Score > Best)
            {
                Best = Score;
                PlayerPrefs.SetInt("StellarShooter.Best", Best);
                PlayerPrefs.Save();
            }
            sound.Play(SoundCue.GameOver);
            SetPhase(GamePhase.GameOver);
        }
        private void SetPhase(GamePhase phase)
        {
            Phase = phase;
            Time.timeScale = phase == GamePhase.Playing || phase == GamePhase.Menu ? 1 : 0;
            ui.Show(phase);
        }
        private void ClearActors()
        {
            // Disable immediately; Destroy is deferred until the end of this frame.
            foreach (Transform child in actors)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }
            Player = null;
        }
        public void AwardKill(int kind, Vector3 position)
        {
            if (!IsPlaying) return;
            Score += GameRules.KillScore(kind);
            kills++;
            // Deterministic drop every fourth kill: spread, repair, shield, repeat.
            if (kills % 4 == 0)
            {
                int index = (kills / 4 - 1) % pickupPrefabs.Length;
                Pickup pickup = Instantiate(pickupPrefabs[index], position, Quaternion.identity, actors);
                pickup.Initialize(this);
            }
        }
        public void Impact(Vector3 position, Color color, float size)
        {
            ImpactEffect effect = Instantiate(impactPrefab, position, Quaternion.identity, actors);
            effect.Initialize(color, size);
        }
        private void OnApplicationFocus(bool focused) { if (!focused && IsPlaying) Pause(); }
        private void OnApplicationPause(bool paused) { if (paused && IsPlaying) Pause(); }
        private void OnDestroy() { Time.timeScale = 1; }
    }
}
