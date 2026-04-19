using UnityEngine;

namespace SpaceShooter.Core
{
    public class AudioManager : MonoBehaviour
    {
        private AudioSource sfxSource;

        private AudioClip shootClip;
        private AudioClip enemyHitClip;
        private AudioClip powerUpClip;
        private AudioClip waveStartClip;
        private AudioClip gameOverClip;

        public void Initialize()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.8f;

            shootClip = LoadOptionalClip("shoot");
            enemyHitClip = LoadOptionalClip("explosion");
            powerUpClip = LoadOptionalClip("powerup");
            waveStartClip = LoadOptionalClip("wave_start");
            gameOverClip = LoadOptionalClip("game_over");
        }

        public void PlayShoot() => Play(shootClip);
        public void PlayEnemyHit() => Play(enemyHitClip);
        public void PlayPowerUp() => Play(powerUpClip);
        public void PlayWaveStart() => Play(waveStartClip);
        public void PlayGameOver() => Play(gameOverClip);

        private void Play(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        private static AudioClip LoadOptionalClip(string clipName)
        {
            return Resources.Load<AudioClip>($"Audio/{clipName}");
        }
    }
}
