// ============================================================
//  AudioManager.cs  –  Fully procedural audio (no asset files)
//  All SFX + background music are synthesised at runtime.
// ============================================================
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Range(0f, 1f)] public float sfxVolume   = 0.70f;
    [Range(0f, 1f)] public float musicVolume = 0.25f;

    AudioSource _sfx;
    AudioSource _music;

    // Cached clips
    AudioClip _clipShoot;
    AudioClip _clipEnemyShoot;
    AudioClip _clipExplosion;
    AudioClip _clipPowerUp;
    AudioClip _clipHit;
    AudioClip _clipBossAlarm;
    AudioClip _clipMusic;

    const int SR = 44100;   // sample rate

    // ── Lifecycle ────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfx   = gameObject.AddComponent<AudioSource>();
        _music = gameObject.AddComponent<AudioSource>();
        _music.loop   = true;
        _music.volume = musicVolume;

        BuildClips();
        _music.clip = _clipMusic;
        _music.Play();
    }

    // ── Public SFX calls ─────────────────────────────────────

    public void PlayShoot()       => _sfx.PlayOneShot(_clipShoot,       sfxVolume);
    public void PlayEnemyShoot()  => _sfx.PlayOneShot(_clipEnemyShoot,  sfxVolume * 0.45f);
    public void PlayExplosion()   => _sfx.PlayOneShot(_clipExplosion,   sfxVolume);
    public void PlayPowerUp()     => _sfx.PlayOneShot(_clipPowerUp,     sfxVolume);
    public void PlayHit()         => _sfx.PlayOneShot(_clipHit,         sfxVolume * 0.6f);
    public void PlayBossAlarm()   => _sfx.PlayOneShot(_clipBossAlarm,   sfxVolume * 0.9f);

    // ── Clip factory ─────────────────────────────────────────

    void BuildClips()
    {
        _clipShoot      = Tone(880f,  0.06f, Wave.Square,   decay: true);
        _clipEnemyShoot = Tone(330f,  0.10f, Wave.Sawtooth, decay: true);
        _clipExplosion  = Noise(0.30f, lowFreq: true);
        _clipPowerUp    = Arpeggio(new[]{ 523f, 659f, 784f, 1047f }, 0.07f);
        _clipHit        = Noise(0.07f, lowFreq: false);
        _clipBossAlarm  = Arpeggio(new[]{ 200f, 150f, 200f, 150f  }, 0.12f);
        _clipMusic      = Music();
    }

    enum Wave { Sine, Square, Sawtooth }

    static AudioClip Tone(float freq, float dur, Wave shape, bool decay = false)
    {
        int n = Mathf.CeilToInt(SR * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t   = (float)i / SR;
            float ph  = 2f * Mathf.PI * freq * t;
            float raw = shape switch
            {
                Wave.Square   => Mathf.Sign(Mathf.Sin(ph)),
                Wave.Sawtooth => 2f * (t * freq - Mathf.Floor(t * freq + 0.5f)),
                _             => Mathf.Sin(ph)
            };
            float env = decay ? Mathf.Pow(1f - (float)i / n, 2f) : 1f;
            d[i] = raw * env * 0.35f;
        }
        return Clip("tone", d);
    }

    static AudioClip Noise(float dur, bool lowFreq)
    {
        int n = Mathf.CeilToInt(SR * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float env  = Mathf.Pow(1f - (float)i / n, 1.5f);
            float s    = (Random.value * 2f - 1f);
            if (lowFreq) s *= Mathf.Sin(Mathf.PI * 60f * (float)i / SR);
            d[i] = s * env * 0.5f;
        }
        return Clip("noise", d);
    }

    static AudioClip Arpeggio(float[] notes, float noteDur)
    {
        int ns = Mathf.CeilToInt(SR * noteDur);
        float[] d = new float[ns * notes.Length];
        for (int b = 0; b < notes.Length; b++)
        {
            float f = notes[b];
            for (int i = 0; i < ns; i++)
            {
                float t   = (float)i / SR;
                float env = Mathf.Pow(1f - (float)i / ns, 1.2f);
                d[b * ns + i] = Mathf.Sin(2 * Mathf.PI * f * t) * env * 0.35f;
            }
        }
        return Clip("arp", d);
    }

    static AudioClip Music()
    {
        // 8-step square-wave melody loop at 140 BPM
        float bpm  = 140f;
        float beat = 60f / bpm;

        float[] mel  = { 261.6f, 329.6f, 392f, 523.3f, 392f, 329.6f, 261.6f, 196f  };
        float[] bass = {  65.4f,  65.4f,  98f, 130.8f,  98f,  65.4f,  65.4f,  49f  };

        int bs = Mathf.CeilToInt(SR * beat);
        float[] d = new float[bs * mel.Length];

        for (int b = 0; b < mel.Length; b++)
        {
            for (int i = 0; i < bs; i++)
            {
                float t   = (float)i / SR;
                float att = Mathf.Min(1f, (float)i / (bs * 0.05f));
                float rel = 1f - Mathf.Max(0f, ((float)i - bs * 0.6f) / (bs * 0.4f));
                float env = att * rel;

                float m  = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * mel[b]  * t)) * 0.18f * env;
                float bv = Mathf.Sign(Mathf.Sin(2 * Mathf.PI * bass[b] * t)) * 0.14f * env;
                d[b * bs + i] = m + bv;
            }
        }
        return Clip("music", d);
    }

    static AudioClip Clip(string name, float[] data)
    {
        var c = AudioClip.Create(name, data.Length, 1, SR, false);
        c.SetData(data, 0);
        return c;
    }
}
