using UnityEngine;

public class SoundInitialization : MonoBehaviour
{
    public string defaultMusicTrack; //name of the music track to play on scene start
    public float defaultMusicVolume = 1f;
    void Start()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMusic(defaultMusicTrack, defaultMusicVolume);
        }
        else
        {
            Debug.LogError("No SoundManager found in scene!");
        }
    }
}
