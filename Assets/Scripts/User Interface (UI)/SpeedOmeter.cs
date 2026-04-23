using UnityEngine;

public class SpeedOmeter : MonoBehaviour
{
    private Transform pivot;
    private NewThirdPlayerMovement pm;
    private Rigidbody playerRb;

    private const float Max_Speed_Angle = -120;
    private const float Zero_Speed_Angle = 118;

    [Header("Settings")]
    public float maxSpeed = 30f; 
    [Tooltip("Higher = faster needle. Lower = heavier/slower needle.")]
    public float needleSmoothness = 5f; 

    private float smoothedSpeedPercent = 0f;

    void Awake()
    {
        pivot = transform.Find("Pivot");
    }

    void Update()
    {
        if (pm == null)
        {
            pm = FindFirstObjectByType<NewThirdPlayerMovement>();
            return;
        }

        if (playerRb == null)
        {
            playerRb = pm.rb ?? pm.GetComponent<Rigidbody>();
            return;
        }

        // Get the raw speed
        Vector3 vel = playerRb.linearVelocity;
        float actualSpeed = new Vector3(vel.x, 0, vel.z).magnitude;

        // Calculate the "Target" percentage (where the needle WANTS to be)
        float targetPercent = Mathf.Clamp01(actualSpeed / maxSpeed);

        // Smoothly move our current percentage toward the target
        // This is what creates the "descending/rising" effect
        smoothedSpeedPercent = Mathf.Lerp(smoothedSpeedPercent, targetPercent, Time.deltaTime * needleSmoothness);

        // Convert that smoothed value into an angle
        float angle = Mathf.Lerp(Zero_Speed_Angle, Max_Speed_Angle, smoothedSpeedPercent);

        // Apply the rotation
        pivot.localRotation = Quaternion.Euler(0, 0, angle);
    }
}