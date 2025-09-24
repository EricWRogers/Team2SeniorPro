using UnityEngine;
using System;
using System.Collections;

public class CarryableAcorn : MonoBehaviour
{
    public Rigidbody rb;
    public Collider col;

    [Header("Carried Behavior")]
    public bool makeTriggerWhileCarried = true;
    public float attachSmoothing = 20f;

    [Header("Catch Gating")]
    public float catchCooldown = 0.35f;
    [HideInInspector] public float nextCatchTime = 0f;

    [Header("Midair / Throw")]
    [Tooltip("How long after a throw the acorn can be midair-caught")]
    public float midairCatchWindow = 1.0f;
    public bool wasThrown { get; private set; } = false;
    public float thrownTime { get; private set; } = -999f;

    [Header("Bounds / Respawn")]
    public float killY = -20f;
    public Transform bottomRespawn;
    [Tooltip("Current respawn point (set by checkpoints). If null, uses bottomRespawn).")]
    public Transform currentRespawn;

    public bool IsCarried { get; private set; }
    Transform carrierSocket;

    // Event: (socketTransform, wasMidairPickup)
    public event Action<Transform, bool> OnPickedUp;

    void Reset() { rb = GetComponent<Rigidbody>(); col = GetComponent<Collider>(); }

    void Start()
    {
        if (!currentRespawn) currentRespawn = bottomRespawn;
    }

    void FixedUpdate()
    {
        if (IsCarried && carrierSocket)
        {
            Vector3 target = carrierSocket.position;
            rb.MovePosition(Vector3.Lerp(rb.position, target, Time.fixedDeltaTime * attachSmoothing));
            rb.MoveRotation(carrierSocket.rotation);
        }

        if (!IsCarried && transform.position.y < killY)
            RespawnToPoint();
    }

    // PickUp optionally ignores cooldown (useful for forced midair catch)
    public void PickUp(Transform socket, bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && Time.time < nextCatchTime) return;

        // determine whether this is a legitimate "midair pickup"
        bool wasMidairPickup = wasThrown && !IsCarried && (Time.time - thrownTime <= midairCatchWindow);

        IsCarried = true;
        carrierSocket = socket;

        rb.isKinematic = true;
        if (makeTriggerWhileCarried) col.isTrigger = true;

        // reset thrown state on successful pickup
        wasThrown = false;

        // Fire event and inform listener whether it was midair
        OnPickedUp?.Invoke(socket, wasMidairPickup);
    }

    // Throw / drop sets thrown state
    public void DropAndThrow(Vector3 v0, Vector3 angular = default)
    {
        IsCarried = false;
        carrierSocket = null;

        nextCatchTime = Time.time + catchCooldown;

        rb.isKinematic = false;
        if (makeTriggerWhileCarried) StartCoroutine(ReenableColliderNextFixed());

        rb.linearVelocity = v0;
        if (angular != Vector3.zero) rb.angularVelocity = angular;

        // mark as thrown and stamp time
        wasThrown = true;
        thrownTime = Time.time;
    }

    IEnumerator ReenableColliderNextFixed() //removes slow down while grounded walking bug
    {
        yield return new WaitForFixedUpdate();
        col.isTrigger = false;
    }

    public void SetRespawnPoint(Transform t) 
    {
        if (t) currentRespawn = t;
    }

    public void ClearRespawnPoint()
    {
        currentRespawn = bottomRespawn;
    }

    public void RespawnToPoint()
    {
        Transform t = currentRespawn ? currentRespawn : bottomRespawn;
        if (!t) return;

        rb.isKinematic = true;
        transform.position = t.position + Vector3.up * 0.2f;
        transform.rotation = t.rotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;

        wasThrown = false;
        nextCatchTime = Time.time + 0.15f; // tiny buffer 
    }

    // Helper to test externally whether this acorn can still be midair-caught
    public bool IsAvailableForMidairCatch()
    {
        return wasThrown && !IsCarried && (Time.time - thrownTime <= midairCatchWindow);
    }
}
