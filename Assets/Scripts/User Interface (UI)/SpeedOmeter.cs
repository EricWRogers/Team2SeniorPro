using UnityEngine;

public class SpeedOmeter : MonoBehaviour
{
    private Transform pivot;
    private NewThirdPlayerMovement pm;

    private const float Max_Speed_Angle = -120;
    private const float Zero_Speed_Angle = 118;

    private void Awake()
    {
        pivot = transform.Find("Pivot");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
