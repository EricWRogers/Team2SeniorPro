using UnityEngine;

public class CatchZone : MonoBehaviour
{
    public Transform handSocket;
    public PlayerCarryState carryState;
    public bool autoCatch = true;
    public float maxCatchSpeed = 2.0f; // only catch if acorn is moving slowly

    void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Acorn")) return;

        var rb = other.attachedRigidbody;
        var ac = rb ? rb.GetComponent<CarryableAcorn>() : null;
        if (ac == null || ac.IsCarried) return;

        // Gate catching: cooldown + speed check + not already carrying
        if (Time.time < ac.nextCatchTime) return;
        if (rb && rb.linearVelocity.sqrMagnitude > maxCatchSpeed * maxCatchSpeed) return;
        if (carryState.IsCarrying) return;

        bool wantCatch = autoCatch || Input.GetKey(KeyCode.E);
        if (!wantCatch) return;

        ac.PickUp(handSocket);
        carryState.SetCarrying(true);
    }
}
