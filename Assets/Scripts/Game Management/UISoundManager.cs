using UnityEngine;
using System.Collections;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }
   
    [Header("Audio Sources")]
    public AudioSource sfxSource;
    

    [Header("Sound Assets")]
    public SoundAsset sfxAssets;

    public float UnmuteDelay = 2f; // adjust in Inspector
    private float lastPlayTime;
    public float hoverCooldown = 0.15f;


    public void PlayButtonSFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.clip = clip;
        sfxSource.PlayOneShot(clip);
        lastPlayTime = Time.unscaledTime; // Mark the time of when played
    }

    public void PlayHoverSFX(AudioClip clip)
    {
        // If the button was just clicked, ignore hover sounds for a split second
        if (Time.unscaledTime - lastPlayTime < hoverCooldown) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetSFXMuted(bool muted)
    {
        if (sfxSource != null)
            sfxSource.mute = muted;
    }

    public void UnmuteSFXDelayed()
    {
        StopAllCoroutines(); // prevents stacking delays
        StartCoroutine(UnmuteSFXAfterDelay());
    }

    public IEnumerator UnmuteSFXAfterDelay()
    {
        yield return new WaitForSecondsRealtime(UnmuteDelay);
        SetSFXMuted(false);
    }

}
