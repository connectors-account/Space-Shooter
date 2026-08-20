using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Core
{
    /// <summary>
    /// Singleton audio hub. Maintains a pool of 10 SFX AudioSources plus a dedicated
    /// music source. If no AudioClips are supplied it synthesizes simple tones with
    /// sine/noise waveforms so the game has sound out of the box with zero assets.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Volumes")]
        [Range(0f, 1f)] public float sfxVolume = 0.6f;
        [Range(0f, 1f)] public float musicVolume = 0.3f;

        private const int SfxSourceCount = 10;
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private int _sfxIndex;
        private AudioSource _musicSource;

        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();

        private const int SampleRate = 44100;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildSources();
            GenerateClips();
        }

        private void BuildSources()
        {
            for (int i = 0; i < SfxSourceCount; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                _sfxSources.Add(src);
            }

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = musicVolume;
        }

        // --- Procedural clip generation ------------------------------------

        private void GenerateClips()
        {
            _clips["shoot"] = MakeTone(0.10f, 900f, 400f, 0.35f, WaveType.Square);
            _clips["boss_shoot"] = MakeTone(0.18f, 300f, 120f, 0.5f, WaveType.Saw);
            _clips["explosion"] = MakeNoise(0.35f, 0.6f, 0.9f);
            _clips["hit"] = MakeTone(0.08f, 500f, 200f, 0.4f, WaveType.Square);
            _clips["powerup"] = MakeArpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.08f, 0.4f);
            _clips["wave_complete"] = MakeArpeggio(new[] { 659f, 784f, 988f, 1318f }, 0.10f, 0.4f);
            _clips["game_over"] = MakeArpeggio(new[] { 440f, 349f, 294f, 220f }, 0.16f, 0.5f);
            _clips["menu_click"] = MakeTone(0.06f, 1200f, 1200f, 0.3f, WaveType.Sine);
        }

        private enum WaveType { Sine, Square, Saw }

        private AudioClip MakeTone(float duration, float startFreq, float endFreq, float amplitude, WaveType type)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            double phase = 0;
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += 2.0 * Mathf.PI * freq / SampleRate;
                float raw;
                switch (type)
                {
                    case WaveType.Square: raw = Mathf.Sign(Mathf.Sin((float)phase)); break;
                    case WaveType.Saw: raw = (float)((phase / (2 * Mathf.PI)) % 1.0) * 2f - 1f; break;
                    default: raw = Mathf.Sin((float)phase); break;
                }
                // Simple attack/decay envelope
                float env = t < 0.1f ? t / 0.1f : 1f - ((t - 0.1f) / 0.9f);
                data[i] = raw * amplitude * Mathf.Clamp01(env);
            }
            return ClipFrom(data, "tone");
        }

        private AudioClip MakeNoise(float duration, float amplitude, float decay)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            var rng = new System.Random(12345);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float env = Mathf.Pow(1f - t, decay * 4f);
                data[i] = ((float)rng.NextDouble() * 2f - 1f) * amplitude * env;
            }
            return ClipFrom(data, "noise");
        }

        private AudioClip MakeArpeggio(float[] freqs, float noteDuration, float amplitude)
        {
            int perNote = Mathf.CeilToInt(SampleRate * noteDuration);
            var data = new float[perNote * freqs.Length];
            int idx = 0;
            foreach (float f in freqs)
            {
                double phase = 0;
                for (int i = 0; i < perNote; i++)
                {
                    phase += 2.0 * Mathf.PI * f / SampleRate;
                    float localT = i / (float)perNote;
                    float env = localT < 0.1f ? localT / 0.1f : 1f - ((localT - 0.1f) / 0.9f);
                    data[idx++] = Mathf.Sin((float)phase) * amplitude * Mathf.Clamp01(env);
                }
            }
            return ClipFrom(data, "arp");
        }

        private AudioClip ClipFrom(float[] data, string name)
        {
            var clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // --- Public API ----------------------------------------------------

        /// <summary>Register or override a named clip (e.g. load real files into Resources).</summary>
        public void RegisterClip(string name, AudioClip clip)
        {
            if (clip != null) _clips[name] = clip;
        }

        public void PlaySFX(string name)
        {
            if (!_clips.TryGetValue(name, out var clip) || clip == null) return;
            var src = _sfxSources[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxSources.Count;
            src.volume = sfxVolume;
            src.PlayOneShot(clip, sfxVolume);
        }

        public void PlayMusic(string name)
        {
            if (!_clips.TryGetValue(name, out var clip) || clip == null) return;
            _musicSource.clip = clip;
            _musicSource.volume = musicVolume;
            _musicSource.Play();
        }

        public void StopMusic()
        {
            if (_musicSource != null) _musicSource.Stop();
        }

        public void SetSFXVolume(float v)
        {
            sfxVolume = Mathf.Clamp01(v);
        }

        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            if (_musicSource != null) _musicSource.volume = musicVolume;
        }
    }
}
