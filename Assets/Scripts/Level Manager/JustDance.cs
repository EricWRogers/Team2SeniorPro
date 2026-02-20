using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

public class JustDance : MonoBehaviour
{
    private Animator playerAnimator;

    [Header("Dance Cam Spline")]
    public SplineAnimate cameraSpline;
    public float splineSpeed = 0.1f;

    private bool hasStarted = false;
    
    void Awake()
    {
        if (cameraSpline == null)
        cameraSpline = GameObject.FindWithTag("DanceCam").GetComponent<SplineAnimate>();
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
    }

    public void StartTheDance()
    {
        hasStarted = true;
        playerAnimator.SetBool("dance", true);
    }

}
