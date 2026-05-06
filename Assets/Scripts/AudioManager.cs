using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Music")]
    [SerializeField] private AudioClip gameplayMusicClip;

    [Header("SFX")]
    [field: SerializeField] public AudioClip PlayerShootClip { get; private set; }
    [field: SerializeField] public AudioClip EnemyShootClip { get; private set; }
    [field: SerializeField] public AudioClip PlayerHitClip { get; private set; }
    [field: SerializeField] public AudioClip PlayerDeathClip { get; private set; }
    [field: SerializeField] public AudioClip EnemyDeathClip { get; private set; }
    [field: SerializeField] public AudioClip PowerUpClip { get; private set; }

    [SerializeField] private float musicVolume = 0.4f;
    [SerializeField] private float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            if (gameplayMusicClip != null)
            {
                musicSource.clip = gameplayMusicClip;
                musicSource.Play();
            }
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
