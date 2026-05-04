using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TVInteraction : MonoBehaviour
{
    [Header("Camera Setup")]
    public Transform tvViewPoint;
    public float transitionSpeed = 0.6f;

    private Camera mainCam;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool isViewing = false;

    // List to remember which scripts we disable to turn them back on
    private List<MonoBehaviour> disabledScripts = new List<MonoBehaviour>();
    
    void Start()
    {
        mainCam = Camera.main;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isViewing)
        {
            EnterView(other.gameObject);
        }
    }
    
    void Update()
    {
        if (isViewing && Input.GetKeyDown(KeyCode.E))
        {
            ExitView();
        }
    }

    void EnterView(GameObject player)
    {
        isViewing = true;
        disabledScripts.Clear();

        // Dynamically find and disable all player scripts
        MonoBehaviour[] scripts = player.GetComponentsInChildren<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // We only disable if it's currently enabled and NOT this script
            if (script.enabled && script != this)
            {
                script.enabled = false;
                disabledScripts.Add(script);
            }
        }

        // Store cam state
        startPos = mainCam.transform.position;
        startRot = mainCam.transform.rotation;

        // Dotween transitions (Ignoring Time.timeScale)
        mainCam.transform.DOMove(tvViewPoint.position, transitionSpeed).SetUpdate(true).SetEase(Ease.OutQuad);
        mainCam.transform.DORotateQuaternion(tvViewPoint.rotation, transitionSpeed).SetUpdate(true).SetEase(Ease.OutQuad);

        // Freeze time
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, transitionSpeed).SetUpdate(true);
    }

    void ExitView()
    {
        // Reverse Camera
        mainCam.transform.DOMove(startPos, transitionSpeed).SetUpdate(true);
        mainCam.transform.DORotateQuaternion(startRot, transitionSpeed).SetUpdate(true).OnComplete(() =>
        {
            // Re-enable previously disabled scripts
            foreach (var script in disabledScripts)
            {
                if (script != null) // Check if the script still exists
                {
                    script.enabled = true;
                }
                isViewing = false;
            }
        });

        // Unfreeze time
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, transitionSpeed).SetUpdate(true);
    }
}
