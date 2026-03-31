using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBackgroundCycler : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite[] backgrounds;
    [SerializeField] private float cycleInterval = 2f;

    private Coroutine cycleRoutine;
    private int currentIndex;
    

    private void OnEnable()
    {
        if (backgroundImage == null || backgrounds == null || backgrounds.Length == 0)
            return;

        currentIndex = Random.Range(0, backgrounds.Length);
        backgroundImage.sprite = backgrounds[currentIndex];

        cycleRoutine = StartCoroutine(CycleRoutine());
    }

    private void OnDisable()
    {
        if (cycleRoutine != null)
        {
            StopCoroutine(cycleRoutine);
            cycleRoutine = null;
        }
    }

    private IEnumerator CycleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(cycleInterval);

            currentIndex++;
            if (currentIndex >= backgrounds.Length)
                currentIndex = 0;

            backgroundImage.sprite = backgrounds[currentIndex];
        }
    }
}