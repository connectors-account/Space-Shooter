using UnityEngine;

public enum AudioCue
{
    Shoot,
    Explosion,
    PlayerHit,
    PowerUp,
    WaveStart,
    GameOver,
    Button
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("SFX")]
    [SerializeField] private AudioClip shoot;
    [SerializeField] private AudioClip explosion;
    [SerializeField] private AudioClip playerHit;
    [SerializeField] private AudioClip powerup;
    [SerializeField] private AudioClip waveStart;
    [SerializeField] private AudioClip gameOver;
    [SerializeField] private AudioClip button;

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
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySfx(AudioCue cue)
    {
        if (sfxSource == null)
        {
            return;
        }

        AudioClip clip = cue switch
        {
            AudioCue.Shoot => shoot,
            AudioCue.Explosion => explosion,
            AudioCue.PlayerHit => playerHit,
            AudioCue.PowerUp => powerup,
            AudioCue.WaveStart => waveStart,
            AudioCue.GameOver => gameOver,
            AudioCue.Button => button,
            _ => null
        };

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}
