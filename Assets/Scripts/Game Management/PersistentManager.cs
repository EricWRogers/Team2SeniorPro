using System.Collections;
using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance;

    private bool hasBooted = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private IEnumerator Start()
    {
        Debug.Log("PersistentManager STARTED");


        /*if (hasBooted) yield break;
        hasBooted = true;

        // Start first load
        yield return StartCoroutine(LevelLoader.Instance.LoadLevelRoutine(
            "Main Menu",
            GetInitializationSteps()
        ));*/

        LevelLoader.Instance.ShowLoadingScreenImmediate();
        yield return null;

        yield return StartCoroutine(LevelLoader.Instance.LoadLevelRoutine(
            "Main Menu",
            GetInitializationSteps()
        ));
    }

    public IEnumerator GetInitializationSteps()
    {
        // These are placeholder tasks for now.
        // Later you can replace them with actual system initialization.

        LevelLoader.Instance.SetLoadingStep("Loading Save Data...");
        yield return new WaitForSeconds(0.4f);
        LevelLoader.Instance.SetExtraProgress(0.2f);

        LevelLoader.Instance.SetLoadingStep("Loading Audio Manager...");
        yield return new WaitForSeconds(0.4f);
        LevelLoader.Instance.SetExtraProgress(0.4f);

        LevelLoader.Instance.SetLoadingStep("Loading UI Systems...");
        yield return new WaitForSeconds(0.4f);
        LevelLoader.Instance.SetExtraProgress(0.6f);

        LevelLoader.Instance.SetLoadingStep("Preparing Game Data...");
        yield return new WaitForSeconds(0.4f);
        LevelLoader.Instance.SetExtraProgress(0.8f);

        LevelLoader.Instance.SetLoadingStep("Finishing Initialization...");
        yield return new WaitForSeconds(0.4f);
        LevelLoader.Instance.SetExtraProgress(1.0f);
    }
}