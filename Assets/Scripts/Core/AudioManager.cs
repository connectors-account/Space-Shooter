using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    [Serializable]
    public class SoundClip
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
    }

    /// <summary>
    /// Singleton audio manager. Plays SFX by name using a pool of AudioSources,
    /// and manages background music with a smooth crossfade. Volumes persist to PlayerPrefs.
    /// Supported SFX/music names: shoot, explosion, powerup, hit, boss_music, menu_music, game_music.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const string MasterKey = "vol_master";
        private const string SfxKey = "vol_sfx";
        private const string MusicKey = "vol_music";

        [Header("Sound Library")]
        [SerializeField] private List<SoundClip> sfxClips = new List<SoundClip>();
        [SerializeField] private List<SoundClip> musicClips = new List<SoundClip>();

        [Header("Settings")]
        [SerializeField] private int sfxSourceCount = 8;
        [SerializeField] private float crossfadeDuration = 1.5f;

        private readonly Dictionary<string, SoundClip> _sfxLookup = new Dictionary<string, SoundClip>();
        private readonly Dictionary<string, SoundClip> _musicLookup = new Dictionary<string, SoundClip>();

        private AudioSource[] _sfxSources;
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private bool _usingSourceA = true;
        private Coroutine _crossfadeRoutine;

        private float _masterVolume = 1f;
        private float _sfxVolume = 1f;
        private float _musicVolume = 0.6f;

        public float MasterVolume => _masterVolume;
        public float SfxVolume => _sfxVolume;
        public float MusicVolume => _musicVolume;

        public event Action OnVolumeChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildLookups();
            CreateSources();
            LoadVolumes();
        }

        private void BuildLookups()
        {
            _sfxLookup.Clear();
            foreach (SoundClip s in sfxClips)
            {
                if (s != null && !string.IsNullOrEmpty(s.name))
                {
                    _sfxLookup[s.name] = s;
                }
            }
            _musicLookup.Clear();
            foreach (SoundClip m in musicClips)
            {
                if (m != null && !string.IsNullOrEmpty(m.name))
                {
                    _musicLookup[m.name] = m;
                }
            }
        }

        private void CreateSources()
        {
            _sfxSources = new AudioSource[Mathf.Max(1, sfxSourceCount)];
            for (int i = 0; i < _sfxSources.Length; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                _sfxSources[i] = src;
            }

            _musicSourceA = gameObject.AddComponent<AudioSource>();
            _musicSourceB = gameObject.AddComponent<AudioSource>();
            foreach (var m in new[] { _musicSourceA, _musicSourceB })
            {
                m.playOnAwake = false;
                m.loop = true;
                m.volume = 0f;
            }
        }

        private void LoadVolumes()
        {
            _masterVolume = PlayerPrefs.GetFloat(MasterKey, 1f);
            _sfxVolume = PlayerPrefs.GetFloat(SfxKey, 1f);
            _musicVolume = PlayerPrefs.GetFloat(MusicKey, 0.6f);
            ApplyMusicVolume();
        }

        /// <summary>Plays a one-shot sound effect by name using a free pooled AudioSource.</summary>
        public void PlaySFX(string clipName)
        {
            if (!_sfxLookup.TryGetValue(clipName, out SoundClip sound) || sound.clip == null)
            {
                return;
            }

            AudioSource source = GetFreeSfxSource();
            source.clip = sound.clip;
            source.volume = sound.volume * _sfxVolume * _masterVolume;
            source.pitch = sound.pitch;
            source.Play();
        }

        private AudioSource GetFreeSfxSource()
        {
            foreach (AudioSource s in _sfxSources)
            {
                if (!s.isPlaying)
                {
                    return s;
                }
            }
            // All busy: reuse the first (steal oldest).
            return _sfxSources[0];
        }

        /// <summary>Crossfades the background music to a new named track.</summary>
        public void PlayMusic(string musicName)
        {
            if (!_musicLookup.TryGetValue(musicName, out SoundClip music) || music.clip == null)
            {
                return;
            }

            AudioSource target = _usingSourceA ? _musicSourceB : _musicSourceA;
            AudioSource current = _usingSourceA ? _musicSourceA : _musicSourceB;

            if (current.isPlaying && current.clip == music.clip)
            {
                return; // Already playing this track.
            }

            target.clip = music.clip;
            target.volume = 0f;
            target.Play();

            if (_crossfadeRoutine != null)
            {
                StopCoroutine(_crossfadeRoutine);
            }
            _crossfadeRoutine = StartCoroutine(Crossfade(current, target, music.volume));
            _usingSourceA = !_usingSourceA;
        }

        private IEnumerator Crossfade(AudioSource from, AudioSource to, float targetTrackVolume)
        {
            float t = 0f;
            float startFrom = from.volume;
            float targetVol = targetTrackVolume * _musicVolume * _masterVolume;
            float duration = Mathf.Max(0.01f, crossfadeDuration);

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / duration;
                from.volume = Mathf.Lerp(startFrom, 0f, k);
                to.volume = Mathf.Lerp(0f, targetVol, k);
                yield return null;
            }

            from.volume = 0f;
            from.Stop();
            to.volume = targetVol;
            _crossfadeRoutine = null;
        }

        public void StopMusic()
        {
            _musicSourceA.Stop();
            _musicSourceB.Stop();
        }

        private void ApplyMusicVolume()
        {
            AudioSource active = _usingSourceA ? _musicSourceA : _musicSourceB;
            if (active != null && active.clip != null)
            {
                // Music clips store per-clip volume implicitly via last crossfade; use full scaling.
                active.volume = _musicVolume * _masterVolume;
            }
        }

        public void SetMasterVolume(float value)
        {
            _masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterKey, _masterVolume);
            ApplyMusicVolume();
            OnVolumeChanged?.Invoke();
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxKey, _sfxVolume);
            OnVolumeChanged?.Invoke();
        }

        public void SetMusicVolume(float value)
        {
            _musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicKey, _musicVolume);
            ApplyMusicVolume();
            OnVolumeChanged?.Invoke();
        }

        private void OnDisable()
        {
            PlayerPrefs.Save();
        }
    }
}
