using UnityEngine;

namespace NovaGuard
{
    public enum SoundCue { Fire, EnemyFire, Hit, Explosion, Pickup, Wave, GameOver, Click }

    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundBank : MonoBehaviour
    {
        public AudioClip[] clips;
        public bool Muted { get; private set; }
        private AudioSource source;
        private float enemySoundAt;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = 0.5f;
            Muted = PlayerPrefs.GetInt("NovaGuard.Muted", 0) == 1;
            source.mute = Muted;
        }

        public void Play(SoundCue cue)
        {
            int i = (int)cue;
            if (Muted || i < 0 || i >= clips.Length || clips[i] == null) return;
            // Dense enemy waves should not drown out damage and pickup feedback.
            if (cue == SoundCue.EnemyFire)
            {
                if (Time.unscaledTime < enemySoundAt) return;
                enemySoundAt = Time.unscaledTime + 0.10f;
            }
            float volume = cue == SoundCue.Fire ? .45f : cue == SoundCue.EnemyFire ? .3f : 1f;
            source.PlayOneShot(clips[i], volume);
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            source.mute = Muted;
            PlayerPrefs.SetInt("NovaGuard.Muted", Muted ? 1 : 0);
            PlayerPrefs.Save();
            if (!Muted) Play(SoundCue.Click);
        }
    }
}
