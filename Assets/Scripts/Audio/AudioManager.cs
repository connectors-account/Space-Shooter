// ============================================================================
// AudioManager.cs — Centralized audio playback (singleton)
// Manages background music and one-shot SFX. Uses string-keyed clips so
// other scripts can call AudioManager.Instance.PlaySFX("Explosion").
// When no AudioClip is assigned the call is silently ignored — allows the
// game to run without audio assets during development.
// ============================================================================
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Audio
{
    [System.Serializable]
    public class SoundEntry
    {
        public string key;           // e.g. "PlayerShoot", "Explosion", "PowerUp"
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Music")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;

        [Header("SFX")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

        private Dictionary<string, SoundEntry> _soundDict;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Build lookup table
            _soundDict = new Dictionary<string, SoundEntry>();
            foreach (var s in sounds)
            {
                if (!string.IsNullOrEmpty(s.key))
                    _soundDict[s.key] = s;
            }

            // Create sources if not assigned
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 0.4f;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        // ---- Music controls ----
        public void PlayMenuMusic()
        {
            PlayMusic(menuMusic);
        }

        public void PlayGameplayMusic()
        {
            PlayMusic(gameplayMusic);
        }

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null || musicSource == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;
            musicSource.clip = clip;
            musicSource.Play();
        }

        // ---- SFX ----
        /// <summary>Play a one-shot sound effect by key. Safe to call even if the clip is missing.</summary>
        public void PlaySFX(string key)
        {
            if (sfxSource == null) return;
            if (_soundDict != null && _soundDict.TryGetValue(key, out var entry) && entry.clip != null)
            {
                sfxSource.PlayOneShot(entry.clip, entry.volume);
            }
        }

        public void SetMusicVolume(float v)
        {
            if (musicSource != null) musicSource.volume = Mathf.Clamp01(v);
        }

        public void SetSFXVolume(float v)
        {
            if (sfxSource != null) sfxSource.volume = Mathf.Clamp01(v);
        }
    }
}
