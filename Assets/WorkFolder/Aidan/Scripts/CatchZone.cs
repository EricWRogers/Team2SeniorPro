using UnityEngine;

public class CatchZone : MonoBehaviour
{
    public Transform handSocket;
    public PlayerCarryState carryState;

    [Header("Behavior")]
    public bool autoCatch = false;         // recommend OFF for readability
    public float maxAutoCatchSpeed = 2.0f; // auto-catch only when slow
    public KeyCode catchKey = KeyCode.E;   // manual catch

    [Header("Debug")]
    public bool debug;

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Acorn")) return;

        var rb = other.attachedRigidbody;
        var ac = rb ? rb.GetComponent<CarryableAcorn>() : null;
        if (!ac) return;

        if (ac.IsCarried) { if (debug) Debug.Log("Catch blocked: already carried."); return; }
        if (carryState.IsCarrying) { if (debug) Debug.Log("Catch blocked: player already carrying."); return; }

        // Respect throw cooldown
        if (Time.time < ac.nextCatchTime) {
            if (debug) Debug.Log($"Catch blocked: cooldown ({ac.nextCatchTime - Time.time:0.00}s left).");
            return;
        }

        // Decide whether the player wants to catch
        bool wantsManualCatch = Input.GetKey(catchKey);
        bool wantsAutoCatch   = autoCatch && rb && rb.linearVelocity.sqrMagnitude <= maxAutoCatchSpeed * maxAutoCatchSpeed;

        if (!(wantsManualCatch || wantsAutoCatch)) {
            if (debug) {
                if (autoCatch) Debug.Log($"No catch: speed {rb.linearVelocity.magnitude:0.00} > auto limit {maxAutoCatchSpeed} and E not held.");
                else Debug.Log("No catch: press E to pick up.");
            }
            return;
        }

        // Manual catch overrides speed gating entirely 
        ac.PickUp(handSocket);
        carryState.SetCarrying(true);
        if (debug) Debug.Log("Caught acorn.");
    }
}
