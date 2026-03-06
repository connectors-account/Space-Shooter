using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all game audio including sound effects and background music.
/// Implements singleton pattern and provides easy-to-use sound playing methods.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
        public bool randomizePitch = false;
        [Range(0f, 0.5f)] public float pitchVariation = 0.1f;
    }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Sound Effects")]
    [SerializeField] private List<SoundEffect> soundEffects = new List<SoundEffect>();
    
    [Header("Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    
    [Header("Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private float musicVolume = 0.5f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private bool sfxEnabled = true;
    
    // Sound effect dictionary for quick lookup
    private Dictionary<string, SoundEffect> soundDictionary;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeAudio();
        LoadSettings();
    }
    
    private void InitializeAudio()
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
        
        // Build sound dictionary
        soundDictionary = new Dictionary<string, SoundEffect>();
        foreach (var sound in soundEffects)
        {
            if (!string.IsNullOrEmpty(sound.name) && !soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary[sound.name] = sound;
            }
        }
        
        // Add default sounds if none configured
        SetupDefaultSounds();
    }
    
    private void SetupDefaultSounds()
    {
        // These would normally have AudioClips assigned in the editor
        // For now, we'll create placeholder entries
        string[] defaultSounds = {
            "PlayerShoot", "EnemyShoot", "PlayerHit", "Explosion",
            "PowerUp", "Heal", "WaveStart", "GameOver", "ButtonClick", "Pause"
        };
        
        foreach (var soundName in defaultSounds)
        {
            if (!soundDictionary.ContainsKey(soundName))
            {
                soundDictionary[soundName] = new SoundEffect
                {
                    name = soundName,
                    clip = null, // Would be assigned in editor
                    volume = 1f,
                    pitch = 1f
                };
            }
        }
    }
    
    /// <summary>
    /// Play a sound effect by name
    /// </summary>
    public void PlaySound(string soundName)
    {
        if (!sfxEnabled) return;
        
        if (soundDictionary.TryGetValue(soundName, out SoundEffect sound))
        {
            if (sound.clip != null)
            {
                float pitch = sound.pitch;
                if (sound.randomizePitch)
                {
                    pitch += Random.Range(-sound.pitchVariation, sound.pitchVariation);
                }
                
                sfxSource.pitch = pitch;
                sfxSource.PlayOneShot(sound.clip, sound.volume * sfxVolume * masterVolume);
            }
            else
            {
                // Generate procedural sound if no clip assigned
                PlayProceduralSound(soundName);
            }
        }
    }
    
    /// <summary>
    /// Play sound at specific position (3D sound)
    /// </summary>
    public void PlaySoundAtPosition(string soundName, Vector3 position)
    {
        if (!sfxEnabled) return;
        
        if (soundDictionary.TryGetValue(soundName, out SoundEffect sound) && sound.clip != null)
        {
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * sfxVolume * masterVolume);
        }
    }
    
    /// <summary>
    /// Play menu music
    /// </summary>
    public void PlayMenuMusic()
    {
        if (menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// Play game music
    /// </summary>
    public void PlayGameMusic()
    {
        if (gameMusic != null)
        {
            musicSource.clip = gameMusic;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
    }
    
    /// <summary>
    /// Stop music
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
        SaveSettings();
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
        SaveSettings();
    }
    
    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveSettings();
    }
    
    /// <summary>
    /// Enable or disable SFX
    /// </summary>
    public void SetSFXEnabled(bool enabled)
    {
        sfxEnabled = enabled;
        SaveSettings();
    }
    
    /// <summary>
    /// Generate simple procedural sounds
    /// </summary>
    private void PlayProceduralSound(string soundName)
    {
        // Create simple procedural audio for demo purposes
        AudioClip clip = null;
        
        switch (soundName)
        {
            case "PlayerShoot":
                clip = GenerateToneClip(880, 0.05f);
                break;
            case "EnemyShoot":
                clip = GenerateToneClip(440, 0.08f);
                break;
            case "Explosion":
                clip = GenerateNoiseClip(0.15f);
                break;
            case "PowerUp":
                clip = GenerateChirpClip(440, 880, 0.2f);
                break;
            case "ButtonClick":
                clip = GenerateToneClip(660, 0.03f);
                break;
        }
        
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }
    }
    
    private AudioClip GenerateToneClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (float)i / samples; // Fade out
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.3f;
        }
        
        AudioClip clip = AudioClip.Create("Tone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
    
    private AudioClip GenerateNoiseClip(float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float envelope = 1f - (float)i / samples;
            data[i] = Random.Range(-1f, 1f) * envelope * 0.3f;
        }
        
        AudioClip clip = AudioClip.Create("Noise", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
    
    private AudioClip GenerateChirpClip(float startFreq, float endFreq, float duration)
    {
        int sampleRate = 44100;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float progress = (float)i / samples;
            float frequency = Mathf.Lerp(startFreq, endFreq, progress);
            float envelope = 1f - progress * 0.5f;
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.3f;
        }
        
        AudioClip clip = AudioClip.Create("Chirp", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("SFXEnabled", sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
        
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
        }
    }
}
