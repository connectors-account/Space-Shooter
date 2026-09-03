using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Central audio hub. Persistent singleton.
    /// Owns a pool of SFX AudioSources for overlap and a dedicated music source.
    /// All clips are generated procedurally by AudioGenerator on Awake.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Singleton
        public static AudioManager Instance { get; private set; }
        #endregion

        #region Sources
        [Header("Audio Sources")]
        [SerializeField] private AudioSource[] _sfxSources;
        [SerializeField] private AudioSource _musicSource;

        [Header("Volumes")]
        [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.4f;

        private int _nextSourceIndex;
        #endregion

        #region Clips
        public AudioClip Shoot { get; private set; }
        public AudioClip EnemyShoot { get; private set; }
        public AudioClip Explosion { get; private set; }
        public AudioClip PowerUp { get; private set; }
        public AudioClip BossRoar { get; private set; }
        public AudioClip WaveComplete { get; private set; }
        public AudioClip ButtonClick { get; private set; }
        public AudioClip PlayerHit { get; private set; }
        public AudioClip Music { get; private set; }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureSources();
            GenerateAndRegisterClips();
        }

        private void Start()
        {
            if (Music != null) PlayMusic(Music, true);
        }
        #endregion

        #region Setup
        private void EnsureSources()
        {
            if (_sfxSources == null || _sfxSources.Length == 0)
            {
                _sfxSources = new AudioSource[GameConstants.SFX_SOURCE_COUNT];
                for (int i = 0; i < _sfxSources.Length; i++)
                {
                    GameObject go = new GameObject($"SFXSource_{i}");
                    go.transform.SetParent(transform, false);
                    AudioSource src = go.AddComponent<AudioSource>();
                    src.playOnAwake = false;
                    src.loop = false;
                    src.spatialBlend = 0f;
                    _sfxSources[i] = src;
                }
            }

            if (_musicSource == null)
            {
                GameObject go = new GameObject("MusicSource");
                go.transform.SetParent(transform, false);
                _musicSource = go.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f;
            }
        }

        private void GenerateAndRegisterClips()
        {
            Shoot = AudioGenerator.GenerateShootSfx();
            EnemyShoot = AudioGenerator.GenerateEnemyShootSfx();
            Explosion = AudioGenerator.GenerateExplosionSfx();
            PowerUp = AudioGenerator.GeneratePowerUpSfx();
            BossRoar = AudioGenerator.GenerateBossRoarSfx();
            WaveComplete = AudioGenerator.GenerateWaveCompleteSfx();
            ButtonClick = AudioGenerator.GenerateButtonClickSfx();
            PlayerHit = AudioGenerator.GeneratePlayerHitSfx();
            Music = AudioGenerator.GenerateBackgroundMusic();
        }
        #endregion

        #region Public API
        /// <summary>Plays a one-shot SFX on the next free source in the pool.</summary>
        public void PlaySFX(AudioClip clip, float pitch = 1f, float volume = 1f)
        {
            if (clip == null || _sfxSources == null || _sfxSources.Length == 0) return;

            AudioSource src = _sfxSources[_nextSourceIndex];
            _nextSourceIndex = (_nextSourceIndex + 1) % _sfxSources.Length;

            src.pitch = pitch;
            src.volume = Mathf.Clamp01(volume) * _sfxVolume;
            src.PlayOneShot(clip);
        }

        /// <summary>Plays a music clip on the dedicated music source.</summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || _musicSource == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = _musicVolume;
            _musicSource.Play();
        }

        /// <summary>Stops the currently playing music.</summary>
        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        /// <summary>Sets the SFX master volume (0-1).</summary>
        public void SetSfxVolume(float v) => _sfxVolume = Mathf.Clamp01(v);

        /// <summary>Sets the music master volume (0-1).</summary>
        public void SetMusicVolume(float v)
        {
            _musicVolume = Mathf.Clamp01(v);
            if (_musicSource != null) _musicSource.volume = _musicVolume;
        }
        #endregion
    }
}
