using UnityEngine;

namespace SpaceShooter.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("SFX")]
        [SerializeField] private AudioClip shootClip;
        [SerializeField] private AudioClip explosionClip;
        [SerializeField] private AudioClip powerUpClip;
        [SerializeField] private AudioClip uiClickClip;

        [Header("Mixer")]
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;

        private AudioSource source;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
        }

        public void PlayShoot() => PlayClip(shootClip, 0.8f);
        public void PlayExplosion() => PlayClip(explosionClip, 0.95f);
        public void PlayPowerUp() => PlayClip(powerUpClip, 0.9f);
        public void PlayUIClick() => PlayClip(uiClickClip, 0.75f);

        private void PlayClip(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip == null || source == null) return;
            source.PlayOneShot(clip, sfxVolume * volumeMultiplier);
        }
    }
}
