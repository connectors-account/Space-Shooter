using UnityEngine;

/// <summary>
/// Manages all game audio. Generates simple procedural sound effects
/// using AudioClips created at runtime (no external audio files needed).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip shootClip;
    private AudioClip explosionClip;
    private AudioClip hitClip;
    private AudioClip powerUpClip;
    private AudioClip menuSelectClip;

    [Range(0f, 1f)] public float sfxVolume = 0.5f;
    [Range(0f, 1f)] public float musicVolume = 0.3f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Create audio sources
        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;

        // Generate procedural sound effects
        GenerateSoundEffects();
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    void GenerateSoundEffects()
    {
        int sampleRate = 44100;

        // Shoot sound - short high-pitched blip
        shootClip = CreateClip("Shoot", sampleRate, 0.1f, (t) =>
        {
            float freq = 880f - t * 4000f;
            float envelope = 1f - t / 0.1f;
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f;
        });

        // Explosion sound - noise burst with decay
        explosionClip = CreateClip("Explosion", sampleRate, 0.4f, (t) =>
        {
            float envelope = Mathf.Exp(-t * 8f);
            float noise = Random.Range(-1f, 1f);
            float bass = Mathf.Sin(2f * Mathf.PI * 60f * t);
            return (noise * 0.5f + bass * 0.5f) * envelope * 0.4f;
        });

        // Hit sound - short thud
        hitClip = CreateClip("Hit", sampleRate, 0.15f, (t) =>
        {
            float envelope = Mathf.Exp(-t * 15f);
            return Mathf.Sin(2f * Mathf.PI * 200f * t) * envelope * 0.3f;
        });

        // Power-up sound - ascending tone
        powerUpClip = CreateClip("PowerUp", sampleRate, 0.3f, (t) =>
        {
            float freq = 400f + t * 2000f;
            float envelope = t < 0.25f ? 1f : (0.3f - t) * 20f;
            envelope = Mathf.Max(0, envelope);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f;
        });

        // Menu select sound
        menuSelectClip = CreateClip("Select", sampleRate, 0.15f, (t) =>
        {
            float freq = 600f;
            float envelope = 1f - t / 0.15f;
            return Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.2f;
        });
    }

    AudioClip CreateClip(string name, int sampleRate, float duration, System.Func<float, float> generator)
    {
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        // We need deterministic random for explosion, so seed it
        System.Random rng = new System.Random(42);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // For noise-based sounds, use deterministic random
            if (name == "Explosion")
            {
                float envelope = Mathf.Exp(-t * 8f);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float bass = Mathf.Sin(2f * Mathf.PI * 60f * t);
                samples[i] = (noise * 0.5f + bass * 0.5f) * envelope * 0.4f;
            }
            else
            {
                samples[i] = generator(t);
            }
        }

        AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    void PlayBackgroundMusic()
    {
        // Generate simple ambient background music
        int sampleRate = 44100;
        float duration = 8f; // 8 second loop
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            // Simple bass drone with subtle melody
            float bass = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.15f;
            float mid = Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.05f;
            float high = Mathf.Sin(2f * Mathf.PI * (220f + Mathf.Sin(t * 0.5f) * 30f) * t) * 0.03f;
            samples[i] = (bass + mid + high) * musicVolume;
        }

        AudioClip musicClip = AudioClip.Create("BGMusic", sampleCount, 1, sampleRate, false);
        musicClip.SetData(samples, 0);
        musicSource.clip = musicClip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PlayShoot() { sfxSource.PlayOneShot(shootClip, sfxVolume); }
    public void PlayExplosion() { sfxSource.PlayOneShot(explosionClip, sfxVolume); }
    public void PlayHit() { sfxSource.PlayOneShot(hitClip, sfxVolume); }
    public void PlayPowerUp() { sfxSource.PlayOneShot(powerUpClip, sfxVolume); }
    public void PlayMenuSelect() { sfxSource.PlayOneShot(menuSelectClip, sfxVolume); }
}
