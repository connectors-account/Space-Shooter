using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages sound effects and background music.
/// Singleton. Attach to a persistent AudioManager GameObject.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffect[] soundEffects;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.3f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 0.7f;

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float pitchVariation = 1f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build dictionary for fast lookup
        if (soundEffects != null)
        {
            foreach (SoundEffect sfx in soundEffects)
            {
                if (sfx.clip != null && !sfxDictionary.ContainsKey(sfx.name))
                {
                    sfxDictionary[sfx.name] = sfx.clip;
                }
            }
        }

        // Create audio sources if not assigned
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    private void Start()
    {
        PlayMusic();
    }

    /// <summary>
    /// Plays a named sound effect.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (sfxSource == null) return;

        // Find the SoundEffect entry for volume/pitch settings
        SoundEffect effect = null;
        if (soundEffects != null)
        {
            foreach (SoundEffect sfx in soundEffects)
            {
                if (sfx.name == sfxName)
                {
                    effect = sfx;
                    break;
                }
            }
        }

        if (effect != null && effect.clip != null)
        {
            float pitch = Random.Range(2f - effect.pitchVariation, effect.pitchVariation);
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(effect.clip, effect.volume * sfxVolume);
        }
        else if (sfxDictionary.ContainsKey(sfxName))
        {
            sfxSource.pitch = 1f;
            sfxSource.PlayOneShot(sfxDictionary[sfxName], sfxVolume);
        }
        // Silently ignore if SFX not found (allows running without audio)
    }

    /// <summary>
    /// Starts playing the background music.
    /// </summary>
    public void PlayMusic()
    {
        if (musicSource == null || backgroundMusic == null) return;
        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    /// <summary>
    /// Stops the background music.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    /// <summary>
    /// Sets SFX volume.
    /// </summary>
    public void SetSFXVolume(float vol)
    {
        sfxVolume = Mathf.Clamp01(vol);
    }

    /// <summary>
    /// Sets music volume.
    /// </summary>
    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        if (musicSource != null) musicSource.volume = musicVolume;
    }
}
