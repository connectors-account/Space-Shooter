using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Lightweight sound manager. Plays named SFX clips via a pool of AudioSources.
/// Attach to an empty GameObject named "SoundManager".
/// Add clips in the Inspector or via RegisterClip().
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip musicClip;
    [Range(0f, 1f)] public float musicVolume = 0.3f;

    [Header("SFX Clips")]
    public SFXEntry[] sfxEntries;

    [System.Serializable]
    public class SFXEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.5f;
    }

    Dictionary<string, SFXEntry> sfxMap = new Dictionary<string, SFXEntry>();
    AudioSource musicSource;
    AudioSource[] sfxSources;
    int sfxIndex;
    const int SFX_POOL = 8;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Build map
        if (sfxEntries != null)
            foreach (var e in sfxEntries)
                if (e.clip != null && !sfxMap.ContainsKey(e.name))
                    sfxMap[e.name] = e;

        // Music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }

        // SFX pool
        sfxSources = new AudioSource[SFX_POOL];
        for (int i = 0; i < SFX_POOL; i++)
            sfxSources[i] = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Play a named SFX. Silently ignored if name not found (no errors).
    /// </summary>
    public void PlaySFX(string name)
    {
        if (!sfxMap.TryGetValue(name, out SFXEntry entry)) return;

        AudioSource src = sfxSources[sfxIndex % SFX_POOL];
        sfxIndex++;
        src.clip = entry.clip;
        src.volume = entry.volume;
        src.Play();
    }

    /// <summary>
    /// Register a clip at runtime (e.g., from a loaded asset).
    /// </summary>
    public void RegisterClip(string name, AudioClip clip, float volume = 0.5f)
    {
        sfxMap[name] = new SFXEntry { name = name, clip = clip, volume = volume };
    }
}
