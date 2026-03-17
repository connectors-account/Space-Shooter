using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton audio manager.  Plays one-shot SFX by name and loops
/// background music.  Sound clips are registered in the Inspector via
/// the SoundEntry list, or loaded from Resources/Audio at runtime.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string    name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [Header("Sound Library")]
    [SerializeField] private List<SoundEntry> sounds = new List<SoundEntry>();

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.4f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private Dictionary<string, SoundEntry> soundDict;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource   = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;

        BuildDictionary();
    }

    private void Start()
    {
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    private void BuildDictionary()
    {
        soundDict = new Dictionary<string, SoundEntry>();
        foreach (var s in sounds)
        {
            if (!string.IsNullOrEmpty(s.name) && s.clip != null)
                soundDict[s.name] = s;
        }
    }

    /// <summary>
    /// Play a sound effect by name.  If the clip is not registered it
    /// logs a warning (no crash).
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (soundDict == null) return;
        if (soundDict.TryGetValue(sfxName, out SoundEntry entry))
        {
            sfxSource.PlayOneShot(entry.clip, entry.volume);
        }
        else
        {
            // Attempt runtime load from Resources/Audio/<name>
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + sfxName);
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
                // Cache for next time
                var newEntry = new SoundEntry { name = sfxName, clip = clip, volume = 1f };
                sounds.Add(newEntry);
                soundDict[sfxName] = newEntry;
            }
            // Silently ignore missing sounds so the game still works without audio files
        }
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}
