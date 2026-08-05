using System.Collections.Generic;
using UnityEngine;
using SpaceShooter.Utilities;

namespace SpaceShooter.Audio
{
    /// <summary>
    /// Singleton audio manager. Maintains a pool of SFX AudioSources (default
    /// size 8) and a single music AudioSource. Clips are loaded from
    /// Resources/Audio by name (see clip-name constants in Constants.cs).
    ///
    /// Missing clips are tolerated gracefully so the game still runs even if
    /// no audio assets have been added yet.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Pool sizes")]
        [SerializeField] private int sfxPoolSize = 8;

        [Header("Volumes (0..1)")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

        // Clip-name constants (mirrors Constants.cs for convenient local access).
        public const string SfxPlayerShoot = Constants.SfxPlayerShoot;
        public const string SfxEnemyShoot  = Constants.SfxEnemyShoot;
        public const string SfxExplosion   = Constants.SfxExplosion;
        public const string SfxPlayerHit   = Constants.SfxPlayerHit;
        public const string SfxPowerUp     = Constants.SfxPowerUp;
        public const string SfxBomb        = Constants.SfxBomb;
        public const string SfxShieldUp    = Constants.SfxShieldUp;
        public const string SfxShieldDown  = Constants.SfxShieldDown;
        public const string SfxUiClick     = Constants.SfxUiClick;
        public const string SfxWaveStart   = Constants.SfxWaveStart;
        public const string SfxBossSpawn   = Constants.SfxBossSpawn;
        public const string MusicMenu      = Constants.MusicMenu;
        public const string MusicGame      = Constants.MusicGame;
        public const string MusicBoss      = Constants.MusicBoss;

        private AudioSource[] _sfxSources;
        private int _sfxCursor;
        private AudioSource _musicSource;

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private string _currentMusic;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load persisted volumes.
            sfxVolume = PlayerPrefs.GetFloat(Constants.PrefsSfxVolume, sfxVolume);
            musicVolume = PlayerPrefs.GetFloat(Constants.PrefsMusicVolume, musicVolume);

            BuildSources();
            LoadClips();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildSources()
        {
            _sfxSources = new AudioSource[Mathf.Max(1, sfxPoolSize)];
            for (int i = 0; i < _sfxSources.Length; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = sfxVolume;
                _sfxSources[i] = src;
            }

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;
        }

        private void LoadClips()
        {
            // Load every AudioClip placed under Resources/Audio.
            var loaded = Resources.LoadAll<AudioClip>("Audio");
            foreach (var clip in loaded)
            {
                if (clip != null && !_clips.ContainsKey(clip.name))
                    _clips[clip.name] = clip;
            }
        }

        // -----------------------------------------------------------------
        // Playback
        // -----------------------------------------------------------------
        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            if (!_clips.TryGetValue(clipName, out var clip) || clip == null)
                return; // Clip not present – silently ignore.

            var src = _sfxSources[_sfxCursor];
            _sfxCursor = (_sfxCursor + 1) % _sfxSources.Length;

            src.volume = sfxVolume * Mathf.Clamp01(volumeScale);
            src.PlayOneShot(clip, 1f);
        }

        public void PlayMusic(string clipName)
        {
            if (string.IsNullOrEmpty(clipName)) return;
            if (_currentMusic == clipName && _musicSource.isPlaying) return;

            if (!_clips.TryGetValue(clipName, out var clip) || clip == null)
            {
                _currentMusic = clipName; // Remember intent even if the clip is missing.
                return;
            }

            _currentMusic = clipName;
            _musicSource.clip = clip;
            _musicSource.volume = musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            _musicSource.Stop();
            _currentMusic = null;
        }

        // -----------------------------------------------------------------
        // Volume control
        // -----------------------------------------------------------------
        public float SfxVolume => sfxVolume;
        public float MusicVolume => musicVolume;

        public void SetSFXVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            foreach (var s in _sfxSources) s.volume = sfxVolume;
            PlayerPrefs.SetFloat(Constants.PrefsSfxVolume, sfxVolume);
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            if (_musicSource != null) _musicSource.volume = musicVolume;
            PlayerPrefs.SetFloat(Constants.PrefsMusicVolume, musicVolume);
            PlayerPrefs.Save();
        }
    }
}
