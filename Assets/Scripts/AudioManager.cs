using UnityEngine;

/// <summary>
/// Simple audio manager for background music and global SFX volume.
/// Singleton — survives scene reloads.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.3f;

    [Header("SFX")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.7f;

    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume = musicVolume;
    }

    private void Start()
    {
        PlayMusic();
    }

    public void PlayMusic()
    {
        if (backgroundMusic == null || _musicSource.isPlaying) return;
        _musicSource.clip = backgroundMusic;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = Mathf.Clamp01(vol);
        _musicSource.volume = musicVolume;
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }
}
