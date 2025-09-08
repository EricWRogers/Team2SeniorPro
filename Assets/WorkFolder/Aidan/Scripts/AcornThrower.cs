using UnityEngine;

public class AcornThrower : MonoBehaviour
{
    public Camera aimCamera;           // main camera
    public Transform handSocket;       // same as CatchZone
    public PlayerCarryState carryState;

    [Header("Throw")]
    public float minThrowSpeed = 6f;
    public float maxThrowSpeed = 16f;
    public float maxChargeTime = 1.0f;
    public LineRenderer arcLine;       
    public int arcPoints = 20;
    public float arcTimeStep = 0.05f;

    CarryableAcorn carried;
    float chargeT; bool charging;

    void Update()
    {
        // detect carried acorn
        if (!carried || !carried.IsCarried)
            carried = FindClosestCarried();

        if (carried && carried.IsCarried)
        {
            if (Input.GetMouseButtonDown(0)) { charging = true; chargeT = 0f; }
            if (charging && Input.GetMouseButton(0)) { chargeT += Time.deltaTime; DrawArc(); }
            if (charging && Input.GetMouseButtonUp(0))
            {
                float t = Mathf.Clamp01(chargeT / maxChargeTime);
                float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);
                Vector3 dir = AimDir();
                Vector3 v0 = dir * speed;
                ClearArc();

                carried.DropAndThrow(v0, Random.insideUnitSphere * 2f);
                carryState.SetCarrying(false);
                charging = false;
            }
        }
    }

    CarryableAcorn FindClosestCarried()
    {
        //  just find any acorn within 2m of hand
        Collider[] hits = Physics.OverlapSphere(handSocket.position, 2f, LayerMask.GetMask("Acorn"));
        foreach (var h in hits)
        {
            var ac = h.attachedRigidbody ? h.attachedRigidbody.GetComponent<CarryableAcorn>() : null;
            if (ac && ac.IsCarried) return ac;
        }
        return null;
    }

    Vector3 AimDir()
    {
        Ray r = aimCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        Vector3 dir = r.direction;
        dir.y = Mathf.Clamp(dir.y + 0.15f, -0.2f, 0.9f); 
        return dir.normalized;
    }

    void DrawArc()
    {
        if (!arcLine || !carried) return;
        float t = Mathf.Clamp01(chargeT / maxChargeTime);
        float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);

        Vector3 p = handSocket.position;
        Vector3 v = AimDir() * speed;

        arcLine.positionCount = arcPoints;
        for (int i = 0; i < arcPoints; i++)
        {
            arcLine.SetPosition(i, p);
            v += Physics.gravity * arcTimeStep;
            p += v * arcTimeStep;
        }
    }

    void ClearArc() { if (arcLine) arcLine.positionCount = 0; }
}