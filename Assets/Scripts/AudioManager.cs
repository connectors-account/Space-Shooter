using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip gameplayMusic; // Add looping gameplay music clip here.

    [Header("SFX")]
    [SerializeField] private AudioClip playerShootClip; // Add player shot clip here.
    [SerializeField] private AudioClip enemyShootClip; // Add enemy shot clip here.
    [SerializeField] private AudioClip playerHitClip; // Add player hit clip here.
    [SerializeField] private AudioClip enemyDeathClip; // Add enemy explosion clip here.
    [SerializeField] private AudioClip playerDeathClip; // Add player death clip here.
    [SerializeField] private AudioClip powerUpClip; // Add power-up pickup clip here.
    [SerializeField] private AudioClip gameOverClip; // Add game-over clip here.

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
        if (musicSource != null && gameplayMusic != null)
        {
            musicSource.clip = gameplayMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayPlayerShoot() => PlaySfx(playerShootClip);
    public void PlayEnemyShoot() => PlaySfx(enemyShootClip);
    public void PlayPlayerHit() => PlaySfx(playerHitClip);
    public void PlayEnemyDeath() => PlaySfx(enemyDeathClip);
    public void PlayPlayerDeath() => PlaySfx(playerDeathClip);
    public void PlayPowerUp() => PlaySfx(powerUpClip);
    public void PlayGameOver() => PlaySfx(gameOverClip);

    private void PlaySfx(AudioClip clip)
    {
        if (sfxSource == null || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
