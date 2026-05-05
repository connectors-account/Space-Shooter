using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter.Sound
{
    /// <summary>
    /// Plays SFX and creates placeholder clips procedurally so the game is audible without imported assets.
    /// Replace generated clips with custom clips in inspector if desired.
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private readonly Dictionary<string, AudioClip> _clips = new();
        private AudioSource _sfxSource;
        private AudioSource _musicSource;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = 0.25f;

            BuildPlaceholderClips();
        }

        public void PlayMusic()
        {
            if (_musicSource.isPlaying) return;
            _musicSource.clip = _clips["music"];
            _musicSource.Play();
        }

        public void PlaySfx(string key)
        {
            if (_clips.TryGetValue(key, out var clip))
            {
                _sfxSource.PlayOneShot(clip, 0.8f);
            }
        }

        private void BuildPlaceholderClips()
        {
            _clips["player_shoot"] = CreateTone(880f, 0.06f);
            _clips["enemy_shoot"] = CreateTone(320f, 0.08f);
            _clips["hit"] = CreateTone(520f, 0.05f);
            _clips["player_hit"] = CreateTone(140f, 0.1f);
            _clips["shield_break"] = CreateTone(240f, 0.12f);
            _clips["explosion"] = CreateNoise(0.18f);
            _clips["powerup"] = CreateTone(1040f, 0.12f);
            _clips["wave"] = CreateTone(660f, 0.16f);
            _clips["game_over"] = CreateTone(110f, 0.4f);
            _clips["music"] = CreateMusicLoop();
        }

        private static AudioClip CreateTone(float frequency, float length)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * length);
            var data = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * Mathf.Lerp(1f, 0f, i / (float)sampleCount) * 0.35f;
            }

            var clip = AudioClip.Create($"tone_{frequency}", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateNoise(float length)
        {
            const int sampleRate = 44100;
            var sampleCount = Mathf.CeilToInt(sampleRate * length);
            var data = new float[sampleCount];
            for (var i = 0; i < sampleCount; i++)
            {
                data[i] = Random.Range(-1f, 1f) * Mathf.Lerp(0.35f, 0f, i / (float)sampleCount);
            }

            var clip = AudioClip.Create("noise", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateMusicLoop()
        {
            const int sampleRate = 44100;
            const float length = 2.8f;
            var sampleCount = Mathf.CeilToInt(sampleRate * length);
            var data = new float[sampleCount];
            var notes = new[] { 220f, 277f, 330f, 440f };

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var noteIndex = Mathf.FloorToInt((t / length) * notes.Length) % notes.Length;
                var freq = notes[noteIndex];
                var wave = Mathf.Sin(2f * Mathf.PI * freq * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * freq * 0.5f * t);
                data[i] = wave * 0.08f;
            }

            var clip = AudioClip.Create("music_loop", sampleCount, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
