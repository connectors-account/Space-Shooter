// =============================================================================
// SoundManager.cs — Audio manager for SFX and music (singleton)
// =============================================================================
using UnityEngine;
using System.Collections.Generic;

namespace SpaceShooter.Managers
{
    /// <summary>
    /// Manages all game audio: background music and sound effects.
    /// Uses a pool of AudioSources for concurrent SFX playback.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private int sfxPoolSize = 8;

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip gameOverMusic;

        [Header("Sound Effects")]
        [SerializeField] private SFXEntry[] sfxEntries;

        [System.Serializable]
        public class SFXEntry
        {
            public string name;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
        }

        private Dictionary<string, SFXEntry> sfxDict = new Dictionary<string, SFXEntry>();
        private AudioSource[] sfxPool;
        private int sfxPoolIndex;

        [Header("Volume")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.5f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSFXPool();
            BuildSFXDictionary();
            LoadVolumeSettings();
        }

        /// <summary>
        /// Creates a pool of AudioSource components for SFX playback.
        /// </summary>
        private void InitializeSFXPool()
        {
            sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                GameObject go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(transform);
                sfxPool[i] = go.AddComponent<AudioSource>();
                sfxPool[i].playOnAwake = false;
            }

            if (musicSource == null)
            {
                GameObject musicGO = new GameObject("MusicSource");
                musicGO.transform.SetParent(transform);
                musicSource = musicGO.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
        }

        /// <summary>
        /// Builds a name-to-clip lookup dictionary from the inspector entries.
        /// </summary>
        private void BuildSFXDictionary()
        {
            sfxDict.Clear();
            if (sfxEntries == null) return;
            foreach (SFXEntry entry in sfxEntries)
            {
                if (!string.IsNullOrEmpty(entry.name) && entry.clip != null)
                {
                    sfxDict[entry.name] = entry;
                }
            }
        }

        /// <summary>
        /// Plays a sound effect by name.
        /// </summary>
        public void PlaySFX(string sfxName)
        {
            if (!sfxDict.ContainsKey(sfxName)) return;

            SFXEntry entry = sfxDict[sfxName];
            AudioSource source = sfxPool[sfxPoolIndex];
            source.clip = entry.clip;
            source.volume = entry.volume * sfxVolume * masterVolume;
            source.Play();

            sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
        }

        /// <summary>
        /// Plays a one-shot SFX from an AudioClip directly.
        /// </summary>
        public void PlaySFXClip(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            AudioSource source = sfxPool[sfxPoolIndex];
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.Play();
            sfxPoolIndex = (sfxPoolIndex + 1) % sfxPool.Length;
        }

        /// <summary>
        /// Plays background music for the specified context.
        /// </summary>
        public void PlayMusic(string context)
        {
            AudioClip clip = null;
            switch (context.ToLower())
            {
                case "menu": clip = menuMusic; break;
                case "gameplay": clip = gameplayMusic; break;
                case "boss": clip = bossMusic; break;
                case "gameover": clip = gameOverMusic; break;
            }

            if (clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the current music.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Sets master volume and saves to PlayerPrefs.
        /// </summary>
        public void SetMasterVolume(float vol)
        {
            masterVolume = Mathf.Clamp01(vol);
            musicSource.volume = musicVolume * masterVolume;
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        }

        /// <summary>
        /// Sets music volume and saves to PlayerPrefs.
        /// </summary>
        public void SetMusicVolume(float vol)
        {
            musicVolume = Mathf.Clamp01(vol);
            musicSource.volume = musicVolume * masterVolume;
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        /// <summary>
        /// Sets SFX volume and saves to PlayerPrefs.
        /// </summary>
        public void SetSFXVolume(float vol)
        {
            sfxVolume = Mathf.Clamp01(vol);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }
    }
}
