using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class JustDance : MonoBehaviour
{
    private Animator playerAnimator;

    [Header("Dance Cam Spline")]
    public SplineAnimate cameraSpline;
    public Camera cam;
    public float splineSpeed = 0.1f;

    private bool hasStarted = false;
    
    void Awake()
    {
        if (cameraSpline == null)
            cameraSpline = GameObject.FindWithTag("DanceCam").GetComponent<SplineAnimate>();
        
        if (cam == null)
            // Find camera in DanceCam tagged "Dance"
            cam = GameObject.FindWithTag("Dance").GetComponentInChildren<Camera>();


    }
    void Start()
    {
        // Get the Animator component attached to the GameObject
        playerAnimator = GetComponent<Animator>();

        // Check if the Animator component was found
        if (playerAnimator == null)
        {
            Debug.LogError("Animator component not found!");
        }

        playerAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

        if (cameraSpline != null)
        {
            cameraSpline.Pause();
        }
    }

    void Update()
    {
        if (!hasStarted)
        {
            StartTheDance();
        }

        if (cameraSpline != null)
        {
            cameraSpline.NormalizedTime += splineSpeed * Time.unscaledDeltaTime;
        }

        if (cam != null)
        {
            // change the FOV randomly between 30 and 90
            cam.fieldOfView = Mathf.Lerp(30f, 90f, Mathf.PingPong(Time.time, 1));
        }
    }

    public void StartTheDance()
    {
        hasStarted = true;
        playerAnimator.SetBool("dance", true);
    }

}
