using UnityEngine;

public class AcornThrower2 : MonoBehaviour
{
    [Header("Cursor")]
    public AimCursor aimCursor;

    [Header("Refs")]
    public Camera aimCamera;
    public Transform handSocket;
    public PlayerCarryState carryState;

    [Header("Throw Settings")]
    public float minThrowSpeed = 6f;
    public float maxThrowSpeed = 16f;   //default 16f
    public float maxChargeTime = 1.0f;

    [Header("Optional: protect player after throw")]
    public Collider[] playerCollidersToIgnore;  // assign your player’s CapsuleCollider etc.
    public float ignorePlayerCollisionTime = 0.5f;


    float chargeT;
    bool charging;
    CarryableAcorn carried;

    void Update()
    {
        // Throw flow 
        if (carryState.IsCarrying)
        {
            if (Input.GetMouseButtonDown(0)) { charging = true; chargeT = 0f; }
            
            if (charging && Input.GetMouseButton(0)) { chargeT += Time.deltaTime; }
            
            if (charging && Input.GetMouseButtonUp(0))
            {
                Vector3 dir; float speed;
                GetThrow(out dir, out speed);

                Vector3 v0 = dir * speed;

                if (!carried) carried = FindFirstObjectByType<CarryableAcorn>();
                if (carried)
                {
                    carried.DropAndThrow(v0, Random.insideUnitSphere * 2f);

                    if (playerCollidersToIgnore != null && playerCollidersToIgnore.Length > 0)
                        StartCoroutine(TemporarilyIgnorePlayer(carried));
                }

                carryState.SetCarrying(false);
                charging = false;
                carried = null;
            }
        }
    }


    void GetThrow(out Vector3 dir, out float speed)
    {
        float t = Mathf.Clamp01(chargeT / maxChargeTime);
        speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);

        Vector3 targetPoint;
        if (aimCursor && aimCursor.gameObject.activeSelf)
            targetPoint = aimCursor.transform.position;
        else
            targetPoint = handSocket.position + aimCamera.transform.forward * 10f;

        dir = (targetPoint - handSocket.position).normalized;
    }

    System.Collections.IEnumerator TemporarilyIgnorePlayer(CarryableAcorn ac)
    {
        if (!ac || !ac.col) yield break;
        foreach (var pc in playerCollidersToIgnore)
            if (pc) Physics.IgnoreCollision(ac.col, pc, true);

        yield return new WaitForSeconds(ignorePlayerCollisionTime);

        foreach (var pc in playerCollidersToIgnore)
            if (pc && ac && ac.col) Physics.IgnoreCollision(ac.col, pc, false);
    }
}