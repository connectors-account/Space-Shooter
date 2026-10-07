using UnityEngine;

namespace StellarShooter
{
    public enum SoundCue { Shoot, EnemyFire, Hit, Explosion, Pickup, Wave, GameOver, Click }
    [RequireComponent(typeof(AudioSource))]
    public sealed class SoundBank : MonoBehaviour
    {
        public AudioClip[] clips;
        public bool Muted { get; private set; }
        private AudioSource source;
        private float lastEnemyShot = -1;
        private void Awake()
        {
            source = GetComponent<AudioSource>();
            Muted = PlayerPrefs.GetInt("StellarShooter.Muted", 0) == 1;
            source.mute = Muted;
        }
        public void Play(SoundCue cue)
        {
            if (cue == SoundCue.EnemyFire)
            {
                if (Time.unscaledTime - lastEnemyShot < .1f) return;
                lastEnemyShot = Time.unscaledTime;
            }
            float volume = cue == SoundCue.Shoot ? .28f : cue == SoundCue.EnemyFire ? .2f : .65f;
            source.PlayOneShot(clips[(int)cue], volume);
        }
        public void ToggleMute()
        {
            Muted = !Muted;
            source.mute = Muted;
            PlayerPrefs.SetInt("StellarShooter.Muted", Muted ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
