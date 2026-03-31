using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBackgroundAnimator : MonoBehaviour
{
    [System.Serializable]
    public class AnimatedBackground
    {
        public string name;
        public Sprite[] frames;
        public float framesPerSecond = 12f;
        public bool loop = true;
    }

    [Header("UI Reference")]
    [SerializeField] private Image backgroundImage;

    [Header("Animated Background Sets")]
    [SerializeField] private AnimatedBackground[] animatedBackgrounds;

    [Header("Optional Static Fallback")]
    [SerializeField] private Sprite fallbackSprite;

    private Coroutine animationRoutine;
    private int currentBackgroundIndex = -1;

    private void OnEnable()
    {
        PlayRandomBackground();
    }

    private void OnDisable()
    {
        StopCurrentAnimation();
    }

    public void PlayRandomBackground()
    {
        StopCurrentAnimation();

        if (backgroundImage == null)
        {
            Debug.LogWarning("LoadingBackgroundAnimator: No background image assigned.");
            return;
        }

        if (animatedBackgrounds == null || animatedBackgrounds.Length == 0)
        {
            if (fallbackSprite != null)
            {
                backgroundImage.sprite = fallbackSprite;
            }

            Debug.LogWarning("LoadingBackgroundAnimator: No animated backgrounds assigned.");
            return;
        }

        currentBackgroundIndex = Random.Range(0, animatedBackgrounds.Length);
        AnimatedBackground chosen = animatedBackgrounds[currentBackgroundIndex];

        if (chosen.frames == null || chosen.frames.Length == 0)
        {
            if (fallbackSprite != null)
            {
                backgroundImage.sprite = fallbackSprite;
            }

            Debug.LogWarning($"LoadingBackgroundAnimator: Background '{chosen.name}' has no frames.");
            return;
        }

        animationRoutine = StartCoroutine(PlayAnimation(chosen));
    }

    private IEnumerator PlayAnimation(AnimatedBackground background)
    {
        int frameIndex = 0;
        float delay = 1f / Mathf.Max(1f, background.framesPerSecond);

        while (true)
        {
            if (backgroundImage != null && background.frames[frameIndex] != null)
            {
                backgroundImage.sprite = background.frames[frameIndex];
            }

            yield return new WaitForSeconds(delay);

            frameIndex++;

            if (frameIndex >= background.frames.Length)
            {
                if (background.loop)
                {
                    frameIndex = 0;
                }
                else
                {
                    frameIndex = background.frames.Length - 1;
                    yield break;
                }
            }
        }
    }

    private void StopCurrentAnimation()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }
}