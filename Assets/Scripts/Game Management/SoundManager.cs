using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource ambientSource;
    [Header("Sound Assets")]
    public SoundAsset musicAssets;
    public SoundAsset sfxAssets;

    [Header("Prefabs")]
    public GameObject spawnableAudioSourcePrefab;

    public float UnmuteDelay = 2f; // adjust in Inspector

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(string audioClipName, float volume = 1f, Vector3 position = default)
    {
        if (sfxAssets == null)
        {
            sfxSource = gameObject.GetComponent<AudioSource>();
        }
        AudioClip clip = sfxAssets.audioClips.Find(c => c.name == audioClipName);
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: AudioClip not found: " + audioClipName + ". Either the name is incorrect or the clip is not added to SoundAsset file.");
            return;
        }
        if (position == default) //if no position provided, play at camera
        {
            sfxSource.PlayOneShot(clip, volume);
            return;
        }
        else
        {
            GameObject tempAudioSource = Instantiate(spawnableAudioSourcePrefab, position, Quaternion.identity);
            AudioSource audioSource = tempAudioSource.GetComponent<AudioSource>();
            audioSource.PlayOneShot(clip, volume);
            Destroy(tempAudioSource, clip.length);
            return;
        }
    }
    public void PlayAmbient(string audioClipName, float volume = 1f)
    {
        if (sfxAssets == null)
        {
            ambientSource = gameObject.GetComponent<AudioSource>();
        }
        AudioClip clip = sfxAssets.audioClips.Find(c => c.name == audioClipName);
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: AudioClip not found: " + audioClipName + ". Either the name is incorrect or the clip is not added to SoundAsset file.");
            return;
        }
        ambientSource.clip = clip;
        ambientSource.volume = volume;
        ambientSource.loop = true;
        ambientSource.Play();
    }
    public void StopAmbient()
    {
        if (ambientSource.isPlaying)
        {
            ambientSource.Stop();
        }
    }

    public void PlayMusic(string audioClipName, float volume = 1f)
    {
        if (musicAssets == null)
        {
            musicSource = gameObject.GetComponent<AudioSource>();
        }
        AudioClip clip = musicAssets.audioClips.Find(c => c.name == audioClipName);
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: AudioClip not found: " + audioClipName + ". Either the name is incorrect or the clip is not added to MusicAsset file.");
            return;
        }
        if (musicSource.isPlaying)
        {
            if (musicSource.clip == clip) return; //if the requested music is already playing, do nothing
            musicSource.Stop();
        }
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = true;
        musicSource.Play();
    }
    public void StopMusic(bool _pause = false) //only really need to use this when forcing a quiet moment, otherwise just change music by calling PlayMusic again
    {
        if (musicSource.isPlaying)
        {
            if (_pause)
            {
                musicSource.Pause();
            }
            else
            {
                musicSource.Stop();
            }
        }
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMusicMuted(bool muted)
    {
        if (musicSource != null)
            musicSource.mute = muted;
    }

    public bool IsMusicMuted()
    {
        return musicSource != null && musicSource.mute;
    }

    public void UnmuteMusicDelayed()
    {
        StopAllCoroutines(); // prevents stacking delays
        StartCoroutine(UnmuteMusicAfterDelay());
    }

    private IEnumerator UnmuteMusicAfterDelay()
    {
        yield return new WaitForSecondsRealtime(UnmuteDelay);
        SetMusicMuted(false);
    }

}