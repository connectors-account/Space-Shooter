using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip backgroundMusic;

    [Header("Sound Effect Clips")]
    public AudioClip shootSound;
    public AudioClip explosionSound;
    public AudioClip playerHitSound;
    public AudioClip powerUpSound;
    public AudioClip shieldBreakSound;
    public AudioClip victorySound;
    public AudioClip gameOverSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SetupAudioSources();
        GeneratePlaceholderSounds();
    }

    void SetupAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        musicSource.volume = musicVolume;

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        sfxSource.volume = sfxVolume;
    }

    void GeneratePlaceholderSounds()
    {
        if (shootSound == null)
            shootSound = CreateToneClip(880f, 0.1f, "ShootSound");

        if (explosionSound == null)
            explosionSound = CreateNoiseClip(0.3f, "ExplosionSound");

        if (playerHitSound == null)
            playerHitSound = CreateToneClip(220f, 0.2f, "PlayerHitSound");

        if (powerUpSound == null)
            powerUpSound = CreateArpeggioClip(new float[] { 440f, 554f, 659f, 880f }, 0.1f, "PowerUpSound");

        if (shieldBreakSound == null)
            shieldBreakSound = CreateToneClip(330f, 0.15f, "ShieldBreakSound");

        if (victorySound == null)
            victorySound = CreateArpeggioClip(new float[] { 523f, 659f, 784f, 1047f }, 0.2f, "VictorySound");

        if (gameOverSound == null)
            gameOverSound = CreateToneClip(110f, 0.5f, "GameOverSound");
    }

    AudioClip CreateToneClip(float frequency, float duration, string name)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateNoiseClip(float duration, string name)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f - (t / duration);
            samples[i] = (Random.value * 2f - 1f) * envelope * 0.3f;
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    AudioClip CreateArpeggioClip(float[] frequencies, float noteDuration, string name)
    {
        int sampleRate = 44100;
        int totalSamples = (int)(sampleRate * noteDuration * frequencies.Length);
        float[] samples = new float[totalSamples];

        for (int note = 0; note < frequencies.Length; note++)
        {
            int startSample = (int)(sampleRate * noteDuration * note);
            int endSample = (int)(sampleRate * noteDuration * (note + 1));

            for (int i = startSample; i < endSample && i < totalSamples; i++)
            {
                float t = (float)(i - startSample) / (sampleRate * noteDuration);
                float envelope = 1f - t;
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequencies[note] * t) * envelope * 0.4f;
            }
        }

        AudioClip clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayShootSound()
    {
        PlaySFX(shootSound);
    }

    public void PlayExplosionSound()
    {
        PlaySFX(explosionSound);
    }

    public void PlayPlayerHitSound()
    {
        PlaySFX(playerHitSound);
    }

    public void PlayPowerUpSound()
    {
        PlaySFX(powerUpSound);
    }

    public void PlayShieldBreakSound()
    {
        PlaySFX(shieldBreakSound);
    }

    public void PlayVictorySound()
    {
        PlaySFX(victorySound);
    }

    public void PlayGameOverSound()
    {
        PlaySFX(gameOverSound);
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
}
