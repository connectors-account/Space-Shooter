using UnityEngine;

/// <summary>
/// Attach to objects that need to play sound effects on events
/// </summary>
public class SoundEffectPlayer : MonoBehaviour
{
    [Header("Sound Effects")]
    public string onEnableSFX;
    public string onDisableSFX;
    public string onDestroySFX;

    public AudioClip onEnableClip;
    public AudioClip onDisableClip;
    public AudioClip onDestroyClip;

    [Range(0f, 1f)]
    public float volume = 1f;

    private void OnEnable()
    {
        PlaySound(onEnableSFX, onEnableClip);
    }

    private void OnDisable()
    {
        PlaySound(onDisableSFX, onDisableClip);
    }

    private void OnDestroy()
    {
        PlaySound(onDestroySFX, onDestroyClip);
    }

    private void PlaySound(string sfxName, AudioClip clip)
    {
        if (AudioManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(sfxName))
            {
                AudioManager.Instance.PlaySFX(sfxName);
            }
            else if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip, volume);
            }
        }
        else if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }

    public void PlaySFX(string sfxName)
    {
        AudioManager.Instance?.PlaySFX(sfxName);
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            AudioManager.Instance?.PlaySFX(clip, volume);
        }
    }
}
