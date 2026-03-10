using UnityEngine;
using System.Collections.Generic;

namespace SpaceShooter.Effects
{
    /// <summary>
    /// Manages game audio - music and sound effects
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        
        [Header("Music Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameMusic;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip gameOverMusic;
        
        [Header("Sound Effects")]
        [SerializeField] private AudioClip playerShoot;
        [SerializeField] private AudioClip enemyShoot;
        [SerializeField] private AudioClip explosion;
        [SerializeField] private AudioClip playerHit;
        [SerializeField] private AudioClip powerUp;
        [SerializeField] private AudioClip buttonClick;
        
        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.5f;
        [SerializeField] private float sfxVolume = 0.7f;
        [SerializeField] private float fadeSpeed = 1f;
        
        private Dictionary<string, AudioClip> sfxClips = new Dictionary<string, AudioClip>();
        private bool isMusicEnabled = true;
        private bool isSFXEnabled = true;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Initialize()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
            
            // Load settings
            LoadSettings();
            
            // Register SFX clips
            RegisterSFX("playerShoot", playerShoot);
            RegisterSFX("enemyShoot", enemyShoot);
            RegisterSFX("explosion", explosion);
            RegisterSFX("playerHit", playerHit);
            RegisterSFX("powerUp", powerUp);
            RegisterSFX("buttonClick", buttonClick);
        }
        
        private void RegisterSFX(string name, AudioClip clip)
        {
            if (clip != null && !sfxClips.ContainsKey(name))
            {
                sfxClips[name] = clip;
            }
        }
        
        public void PlayMusic(string trackName)
        {
            AudioClip clip = null;
            
            switch (trackName.ToLower())
            {
                case "menu":
                    clip = menuMusic;
                    break;
                case "game":
                    clip = gameMusic;
                    break;
                case "boss":
                    clip = bossMusic;
                    break;
                case "gameover":
                    clip = gameOverMusic;
                    break;
            }
            
            if (clip != null && musicSource != null)
            {
                musicSource.clip = clip;
                musicSource.volume = isMusicEnabled ? musicVolume : 0f;
                musicSource.Play();
            }
        }
        
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
        
        public void PlaySFX(string sfxName, float volumeMultiplier = 1f)
        {
            if (!isSFXEnabled) return;
            
            if (sfxClips.TryGetValue(sfxName, out AudioClip clip))
            {
                if (sfxSource != null)
                {
                    sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
                }
            }
        }
        
        public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (!isSFXEnabled || clip == null) return;
            
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
            }
        }
        
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null && isMusicEnabled)
            {
                musicSource.volume = musicVolume;
            }
            SaveSettings();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveSettings();
        }
        
        public void ToggleMusic(bool enabled)
        {
            isMusicEnabled = enabled;
            if (musicSource != null)
            {
                musicSource.volume = enabled ? musicVolume : 0f;
            }
            SaveSettings();
        }
        
        public void ToggleSFX(bool enabled)
        {
            isSFXEnabled = enabled;
            SaveSettings();
        }
        
        private void LoadSettings()
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
            isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            isSFXEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        }
        
        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("SFXEnabled", isSFXEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
