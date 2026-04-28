using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoSceneController : MonoBehaviour
{
    [Header("Settings")]
    public string sceneToLoad;
    public float skipButtonDelay = 3f; // Time in seconds before the skip button appears

    [Header("References")]
    public VideoPlayer videoPlayer;
    public GameObject nextButton;
    public GameObject skipButton;

    void Start()
    {
        // Ensure both buttons are hidden at the start
        if (nextButton != null) nextButton.SetActive(false);
        if (skipButton != null) skipButton.SetActive(false);

        // Subscribe to the event that fires when the video finishes playing
        videoPlayer.loopPointReached += OnVideoFinished;

        // Start the timer to show the skip button after applied seconds
        Invoke(nameof(ShowSkipButton), skipButtonDelay);

    }

    void ShowSkipButton()
    {
        // Only show the skip button if the video is still playing
        if (skipButton != null && videoPlayer.isPlaying)
        {
            skipButton.SetActive(true);
        }
    }

    public void SkipVideo()
    {
        // Jump to the very end of the video
        // Set time to just before the end to trigger the loopPointReached event
        videoPlayer.time = videoPlayer.length -0.1f;

        // Hide the skip button immediately after skipping
        if (skipButton != null) skipButton.SetActive(false);
    }

    void OnVideoFinished(VideoPlayer vp)
    {
       // Show the next button when the video finishes
        if (nextButton != null) nextButton.SetActive(true);
        // Hide the skip button if it's still visible
        if (skipButton != null) skipButton.SetActive(false);

        // Optionally, you can also unsubscribe from the event if it's no longer needed
        videoPlayer.loopPointReached -= OnVideoFinished;
    }

    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            GameManager.Instance.newMap(sceneToLoad, true);
        }
        else
        {
            Debug.LogError("Scene name is missing in the Inspector.");
        }

         if (SoundManager.Instance != null)
            SoundManager.Instance.UnmuteMusicDelayed();
    }
}
