using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource sfxSource;

    [Header("Clips")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip waveStartClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip buttonClickClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    public void PlayShoot() => PlayClip(shootClip, 0.7f);
    public void PlayExplosion() => PlayClip(explosionClip, 0.9f);
    public void PlayPlayerHit() => PlayClip(playerHitClip, 0.85f);
    public void PlayPowerUp() => PlayClip(powerUpClip, 0.85f);
    public void PlayWaveStart() => PlayClip(waveStartClip, 0.9f);
    public void PlayGameOver() => PlayClip(gameOverClip, 1f);
    public void PlayButtonClick() => PlayClip(buttonClickClip, 0.7f);

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }
}
