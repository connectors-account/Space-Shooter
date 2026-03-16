using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SoundManager handles all audio playback including music and sound effects.
/// Uses object pooling for efficient sound effect playback.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // Singleton instance
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    [SerializeField] private List<SoundEffect> soundEffects;

    [Header("Music Tracks")]
    [SerializeField] private List<MusicTrack> musicTracks;

    [Header("Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    [SerializeField] private float defaultMusicVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.8f;

    // Private variables
    private Dictionary<string, AudioClip> sfxDictionary;
    private Dictionary<string, AudioClip> musicDictionary;
    private List<AudioSource> sfxPool;
    private float musicVolume;
    private float sfxVolume;

    /// <summary>
    /// Sound effect data class
    /// </summary>
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool randomizePitch = false;
        [Range(0f, 0.5f)] public float pitchVariation = 0.1f;
    }

    /// <summary>
    /// Music track data class
    /// </summary>
    [System.Serializable]
    public class MusicTrack
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = true;
    }

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    /// <summary>
    /// Initialize the sound manager
    /// </summary>
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

        // Build dictionaries
        sfxDictionary = new Dictionary<string, AudioClip>();
        foreach (var sfx in soundEffects)
        {
            if (sfx.clip != null && !string.IsNullOrEmpty(sfx.name))
            {
                sfxDictionary[sfx.name] = sfx.clip;
            }
        }

        musicDictionary = new Dictionary<string, AudioClip>();
        foreach (var track in musicTracks)
        {
            if (track.clip != null && !string.IsNullOrEmpty(track.name))
            {
                musicDictionary[track.name] = track.clip;
            }
        }

        // Create SFX pool
        CreateSFXPool();

        // Load volume settings
        LoadVolumeSettings();
    }

    /// <summary>
    /// Create object pool for sound effects
    /// </summary>
    private void CreateSFXPool()
    {
        sfxPool = new List<AudioSource>();
        GameObject poolContainer = new GameObject("SFXPool");
        poolContainer.transform.SetParent(transform);

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFXSource_{i}");
            sfxObj.transform.SetParent(poolContainer.transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }

    /// <summary>
    /// Load volume settings from PlayerPrefs
    /// </summary>
    private void LoadVolumeSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);
        
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    public void PlaySound(string soundName)
    {
        // Find the sound effect
        SoundEffect sfx = soundEffects.Find(s => s.name == soundName);
        if (sfx == null || sfx.clip == null)
        {
            Debug.LogWarning($"Sound '{soundName}' not found!");
            return;
        }

        // Get available audio source from pool
        AudioSource source = GetAvailableSFXSource();
        if (source == null)
        {
            Debug.LogWarning("No available SFX sources in pool!");
            return;
        }

        // Setup and play
        source.clip = sfx.clip;
        source.volume = sfx.volume * sfxVolume;
        source.pitch = sfx.randomizePitch 
            ? sfx.pitch + Random.Range(-sfx.pitchVariation, sfx.pitchVariation) 
            : sfx.pitch;
        source.Play();
    }

    /// <summary>
    /// Play a sound effect at a specific position (3D sound)
    /// </summary>
    public void PlaySoundAtPosition(string soundName, Vector3 position)
    {
        SoundEffect sfx = soundEffects.Find(s => s.name == soundName);
        if (sfx == null || sfx.clip == null) return;

        AudioSource.PlayClipAtPoint(sfx.clip, position, sfx.volume * sfxVolume);
    }

    /// <summary>
    /// Get an available audio source from the pool
    /// </summary>
    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        
        // If all sources are busy, return the first one (will cut off oldest sound)
        return sfxPool.Count > 0 ? sfxPool[0] : null;
    }

    /// <summary>
    /// Play music by name
    /// </summary>
    public void PlayMusic(string trackName)
    {
        MusicTrack track = musicTracks.Find(t => t.name == trackName);
        if (track == null || track.clip == null)
        {
            Debug.LogWarning($"Music track '{trackName}' not found!");
            return;
        }

        if (musicSource.clip == track.clip && musicSource.isPlaying)
        {
            return; // Already playing this track
        }

        musicSource.clip = track.clip;
        musicSource.volume = track.volume * musicVolume;
        musicSource.loop = track.loop;
        musicSource.Play();
    }

    /// <summary>
    /// Stop current music
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }

    /// <summary>
    /// Pause current music
    /// </summary>
    public void PauseMusic()
    {
        musicSource.Pause();
    }

    /// <summary>
    /// Resume paused music
    /// </summary>
    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    /// <summary>
    /// Set music volume (0-1)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    /// <summary>
    /// Set SFX volume (0-1)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    /// <summary>
    /// Get current music volume
    /// </summary>
    public float GetMusicVolume() => musicVolume;

    /// <summary>
    /// Get current SFX volume
    /// </summary>
    public float GetSFXVolume() => sfxVolume;

    /// <summary>
    /// Fade music in
    /// </summary>
    public void FadeMusicIn(string trackName, float duration)
    {
        StartCoroutine(FadeMusicCoroutine(trackName, duration, true));
    }

    /// <summary>
    /// Fade music out
    /// </summary>
    public void FadeMusicOut(float duration)
    {
        StartCoroutine(FadeMusicCoroutine(null, duration, false));
    }

    /// <summary>
    /// Coroutine for fading music
    /// </summary>
    private System.Collections.IEnumerator FadeMusicCoroutine(string trackName, float duration, bool fadeIn)
    {
        if (fadeIn && !string.IsNullOrEmpty(trackName))
        {
            MusicTrack track = musicTracks.Find(t => t.name == trackName);
            if (track != null)
            {
                musicSource.clip = track.clip;
                musicSource.volume = 0f;
                musicSource.Play();
            }
        }

        float startVolume = fadeIn ? 0f : musicSource.volume;
        float targetVolume = fadeIn ? musicVolume : 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = targetVolume;

        if (!fadeIn)
        {
            musicSource.Stop();
        }
    }
}
