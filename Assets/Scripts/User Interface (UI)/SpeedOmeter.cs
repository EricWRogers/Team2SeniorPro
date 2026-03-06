using UnityEngine;

public class SpeedOmeter : MonoBehaviour
{
    private Transform pivot;
    private NewThirdPlayerMovement pm;

    private const float Max_Speed_Angle = -120;
    private const float Zero_Speed_Angle = 118;

    public float maxSpeed = 30f; // speed that maxes out the guage

    private void Awake()
    {
        pivot = transform.Find("Pivot");

        // Find the player movement script 
        pm = FindFirstObjectByType<NewThirdPlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        if (pm == null) return;

        // Get player's horizontal speed
        Vector3 flatVel = new Vector3(pm.rb.linearVelocity.x, 0f, pm.rb.linearVelocity.z);
        float speed = flatVel.magnitude;

        // Normalize speed (0 to 1)
        float speedPercent = Mathf.Clamp01(speed / maxSpeed);

        // Convert to needle angle
        float angle = Mathf.Lerp(Zero_Speed_Angle, Max_Speed_Angle, speedPercent);

        // Rotate needle
        Quaternion target = Quaternion.Euler(0, 0, angle);
        pivot.localRotation = Quaternion.Lerp(pivot.localRotation, target, Time.deltaTime * 8f);
    }
}
