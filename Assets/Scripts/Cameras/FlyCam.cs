using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class FlyCam : MonoBehaviour
{
    [Header("Flyover Settings")]
    public Transform[] waypoints;
    public float moveSpeed = 2f;
    public float rotateSpeed = 2f;
    public float pauseTime = 1f;

    [Header("References")]
    public GameObject player;
    public Camera playerCam;

    private bool isPlaying = false;

    // Static flag so the flyover only runs once, even if the scene reloads
    private static bool hasPlayedFlyover_Level1 = false;

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Only play in Level_1, and only if it hasn't played yet
        if (currentScene == "Level_1" && !hasPlayedFlyover_Level1)
        {
            hasPlayedFlyover_Level1 = true;
            StartCoroutine(FlyoverRoutine());
        }
        else
        {
            // Skip directly to gameplay
            EndFlyoverInstant();
        }
    }

    IEnumerator FlyoverRoutine()
    {
        if (waypoints.Length == 0) yield break;
        isPlaying = true;

        // Disable player controls during flyover
        player.SetActive(false);
        playerCam.enabled = false;
        gameObject.SetActive(true);

        // Start at first waypoint
        transform.position = waypoints[0].position;
        transform.rotation = waypoints[0].rotation;

        for (int i = 1; i < waypoints.Length; i++)
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;
            Vector3 endPos = waypoints[i].position;
            Quaternion endRot = waypoints[i].rotation;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                transform.rotation = Quaternion.Slerp(startRot, endRot, t * rotateSpeed);
                yield return null;
            }

            yield return new WaitForSeconds(pauseTime);
        }

        EndFlyover();
    }

    void EndFlyover()
    {
        gameObject.SetActive(false);
        player.SetActive(true);
        playerCam.enabled = true;
        isPlaying = false;
    }

    void EndFlyoverInstant()
    {
        gameObject.SetActive(false);
        player.SetActive(true);
        playerCam.enabled = true;
        isPlaying = false;
    }
}
