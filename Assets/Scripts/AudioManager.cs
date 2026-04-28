using System.Collections.Generic;
using UnityEngine;

public enum AudioSfx
{
    Shoot,
    Explosion,
    PlayerHit,
    PowerUpCollected,
    WaveStart,
    GameOver,
    Win,
    ButtonClick
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private AudioClip playerHitClip;
    [SerializeField] private AudioClip powerUpClip;
    [SerializeField] private AudioClip waveStartClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip buttonClickClip;

    private Dictionary<AudioSfx, AudioClip> sfxMap;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        sfxMap = new Dictionary<AudioSfx, AudioClip>
        {
            { AudioSfx.Shoot, shootClip },
            { AudioSfx.Explosion, explosionClip },
            { AudioSfx.PlayerHit, playerHitClip },
            { AudioSfx.PowerUpCollected, powerUpClip },
            { AudioSfx.WaveStart, waveStartClip },
            { AudioSfx.GameOver, gameOverClip },
            { AudioSfx.Win, winClip },
            { AudioSfx.ButtonClick, buttonClickClip }
        };
    }

    public void PlaySfx(AudioSfx sfx)
    {
        if (sfxSource == null || !sfxMap.TryGetValue(sfx, out AudioClip clip) || clip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic()
    {
        if (musicSource == null || backgroundMusic == null)
        {
            return;
        }

        if (musicSource.clip != backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
}
