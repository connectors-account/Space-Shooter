using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all game audio: background music and sound effects.
/// Singleton accessible via AudioManager.Instance.
/// Add AudioClips in the Inspector or load from Resources/Audio.
/// </summary>
[System.Serializable]
public class SoundEffect
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)]
    public float volume = 1f;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.3f;

    [Header("Sound Effects")]
    public List<SoundEffect> soundEffects = new List<SoundEffect>();

    [Header("Settings")]
    public int sfxPoolSize = 10;

    private AudioSource musicSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private Dictionary<string, SoundEffect> sfxLookup = new Dictionary<string, SoundEffect>();
    private int currentSfxIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
        BuildLookup();
    }

    void SetupAudioSources()
    {
        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        // SFX pool
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            sfxSources.Add(src);
        }
    }

    void BuildLookup()
    {
        sfxLookup.Clear();
        foreach (var sfx in soundEffects)
        {
            if (!string.IsNullOrEmpty(sfx.name) && sfx.clip != null)
            {
                sfxLookup[sfx.name] = sfx;
            }
        }
    }

    void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Play a named sound effect. If the clip isn't registered, it silently skips.
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (!sfxLookup.ContainsKey(sfxName)) return;

        SoundEffect sfx = sfxLookup[sfxName];
        AudioSource source = sfxSources[currentSfxIndex];
        source.clip = sfx.clip;
        source.volume = sfx.volume;
        source.Play();

        currentSfxIndex = (currentSfxIndex + 1) % sfxSources.Count;
    }

    /// <summary>
    /// Play a clip directly without registering it.
    /// </summary>
    public void PlayClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        AudioSource source = sfxSources[currentSfxIndex];
        source.clip = clip;
        source.volume = volume;
        source.Play();

        currentSfxIndex = (currentSfxIndex + 1) % sfxSources.Count;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
