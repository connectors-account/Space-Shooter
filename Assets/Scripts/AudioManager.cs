using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter
{
    /// <summary>
    /// Centralized SFX/music playback manager.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public enum SfxType
        {
            PlayerShoot,
            EnemyShoot,
            Explosion,
            PlayerHit,
            PowerUp,
            WaveStart,
            GameOver,
            Hit,
            ButtonClick
        }

        [System.Serializable]
        public struct SfxBinding
        {
            public SfxType type;
            public AudioClip clip;
        }

        public static AudioManager Instance { get; private set; }

        [Header("Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource musicSource;

        [Header("SFX Library")]
        [SerializeField] private SfxBinding[] sfxBindings;

        [Header("Music")]
        [SerializeField] private AudioClip gameplayMusic;

        private readonly Dictionary<SfxType, AudioClip> sfxMap = new Dictionary<SfxType, AudioClip>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            BuildSfxMap();
        }

        private void BuildSfxMap()
        {
            sfxMap.Clear();

            foreach (SfxBinding binding in sfxBindings)
            {
                if (binding.clip != null)
                {
                    sfxMap[binding.type] = binding.clip;
                }
            }
        }

        public void PlaySfx(SfxType type)
        {
            if (sfxSource == null)
            {
                return;
            }

            if (!sfxMap.TryGetValue(type, out AudioClip clip) || clip == null)
            {
                return;
            }

            sfxSource.PlayOneShot(clip);
        }

        public void PlayMusicLoop()
        {
            if (musicSource == null || gameplayMusic == null)
            {
                return;
            }

            if (musicSource.isPlaying && musicSource.clip == gameplayMusic)
            {
                return;
            }

            musicSource.clip = gameplayMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }
    }
}
