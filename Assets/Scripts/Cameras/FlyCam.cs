using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;

public class FlyCam : MonoBehaviour
{
    [Header("References")]
    public Camera flyCam;
    public Camera playerCam;
    public GameObject player;
    public CinemachineCamera playerCineCam;
    public Transform pathParent;      // parent object that holds all waypoint children
    public Image fadeImage;
    public MonoBehaviour pauseScript;

    [Header("Settings")]
    public float totalDuration = 10f;   // total time for full flyover
    public float delayBeforeStart = 1f;
    public float delayAfterFinish = 1.5f;
    public float lookSpeed = 2f;
    public float tiltDown = 10f;
    public float fadeDuration = 1f;

    private Transform[] pathPoints;
    private static bool hasPlayedFlyover_Level1 = false;
    private float timer = 0f;
    private bool isFlying = false;
    private MonoBehaviour[] playerScripts;

    void Start()
    {
        // Collect path points from pathParent if assigned
        if (pathParent != null && pathParent.childCount > 0)
        {
            pathPoints = new Transform[pathParent.childCount];
            for (int i = 0; i < pathParent.childCount; i++)
                pathPoints[i] = pathParent.GetChild(i);
        }

        string currentScene = GameManager.Instance.currentScene;

        if (currentScene == "Squirrel_HUB" && !hasPlayedFlyover_Level1)
        {
            hasPlayedFlyover_Level1 = true;
            StartFlyover();
        }
        else
        {
            EndFlyover();
        }
    }

    void StartFlyover()
    {
        if (pathPoints == null || pathPoints.Length < 2)
        {
            Debug.LogWarning("Not enough path points for FlyCam.");
            EndFlyover();
            return;
        }

        // Disable player scripts
        playerScripts = player.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in playerScripts)
            if (script != null) script.enabled = false;

        // Disable Cinemachine camera and pause script
        if (playerCineCam != null) playerCineCam.enabled = false;
        if (pauseScript != null) pauseScript.enabled = false;

        // Switch to FlyCam
        playerCam.enabled = false;
        flyCam.enabled = true;

        // Set starting position + look at first waypoint
        flyCam.transform.position = pathPoints[0].position;
        flyCam.transform.LookAt(pathPoints[1].position);
        flyCam.transform.rotation *= Quaternion.Euler(tiltDown, 0f, 0f);

        // Fade in from black
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            StartCoroutine(FadeUI(fadeImage, 1f, 0f, fadeDuration));
        }

        Invoke(nameof(BeginFlying), delayBeforeStart);
    }

    void BeginFlying()
    {
        isFlying = true;
        timer = 0f;
    }

    void Update()
    {
        if (!isFlying || pathPoints.Length < 2) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / totalDuration);

        int index = Mathf.FloorToInt(t * (pathPoints.Length - 1));
        int nextIndex = Mathf.Min(index + 1, pathPoints.Length - 1);
        float localT = t * (pathPoints.Length - 1) - index;

        // Move smoothly between path points
        flyCam.transform.position = Vector3.Lerp(
            pathPoints[index].position,
            pathPoints[nextIndex].position,
            localT
        );

        // Smooth rotation toward next point
        Vector3 dir = (pathPoints[nextIndex].position - flyCam.transform.position).normalized;
        if (dir.sqrMagnitude > 0f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            targetRot *= Quaternion.Euler(tiltDown, 0f, 0f);
            flyCam.transform.rotation = Quaternion.Slerp(flyCam.transform.rotation, targetRot, Time.deltaTime * lookSpeed);
        }

        // End flyover
        if (t >= 1f)
        {
            isFlying = false;
            StartCoroutine(EndSequence());
        }
    }

    IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(delayAfterFinish);

        // Fade to black
        if (fadeImage != null)
            yield return FadeUI(fadeImage, 0f, 1f, fadeDuration);

        // Re-enable player scripts, pause, Cinemachine
        EndFlyover();

        // Fade back in
        if (fadeImage != null)
            yield return FadeUI(fadeImage, 1f, 0f, fadeDuration);

        // Fade out FlyCam GameObject + all children
        yield return FadeOutChildren(gameObject, 1.5f);
    }

    void EndFlyover()
    {
        if (playerScripts != null)
            foreach (var script in playerScripts)
                if (script != null) script.enabled = true;

        if (pauseScript != null) pauseScript.enabled = true;
        if (playerCineCam != null) playerCineCam.enabled = true;

        flyCam.enabled = false;
        playerCam.enabled = true;
    }

    // UI fade coroutine
    IEnumerator FadeUI(Image image, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = image.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            image.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }
        image.color = new Color(c.r, c.g, c.b, to);
    }

    // URP fade-out for GameObject + all children
    IEnumerator FadeOutChildren(GameObject target, float duration)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) yield break;

        Material[] mats = new Material[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            mats[i] = renderers[i].material;

            // Ensure transparent surface for URP Lit shader
            if (mats[i].HasProperty("_Surface"))
            {
                mats[i].SetFloat("_Surface", 1f); // 1 = Transparent
                mats[i].EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);

            foreach (var mat in mats)
                if (mat.HasProperty("_BaseColor"))
                {
                    Color c = mat.GetColor("_BaseColor");
                    c.a = alpha;
                    mat.SetColor("_BaseColor", c);
                }

            yield return null;
        }

        // Ensure fully invisible
        foreach (var mat in mats)
            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = 0f;
                mat.SetColor("_BaseColor", c);
            }

        // Disable the FlyCam GameObject
        target.SetActive(false);
    }
}