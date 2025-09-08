using UnityEngine;
using System.Collections;

public class CarryableAcorn : MonoBehaviour
{
    public Rigidbody rb;
    public Collider col;

    [Header("Carried Behavior")]
    public bool makeTriggerWhileCarried = true; // avoids pushing the player
    public float attachSmoothing = 20f;

    [Header("Catch Gating")]
    public float catchCooldown = 0.35f;       // block re-catch right after throw
    [HideInInspector] public float nextCatchTime = 0f;

    [Header("Bounds")]
    public float killY = -20f;
    public Transform bottomRespawn;

    public bool IsCarried { get; private set; }
    Transform carrierSocket;

    void Reset() { rb = GetComponent<Rigidbody>(); col = GetComponent<Collider>(); }

    void FixedUpdate()
    {
        if (IsCarried && carrierSocket)
        {
            Vector3 target = carrierSocket.position;
            rb.MovePosition(Vector3.Lerp(rb.position, target, Time.fixedDeltaTime * attachSmoothing));
            rb.MoveRotation(carrierSocket.rotation);
        }

        if (!IsCarried && transform.position.y < killY)
            RespawnToBottom();
    }

    public void PickUp(Transform socket)
    {
        // Respect cooldown (safety if CatchZone forgot to check)
        if (Time.time < nextCatchTime) return;

        IsCarried = true;
        carrierSocket = socket;

        rb.isKinematic = true;
        if (makeTriggerWhileCarried) col.isTrigger = true;
    }

    public void DropAndThrow(Vector3 v0, Vector3 angular = default)
    {
        IsCarried = false;
        carrierSocket = null;

        nextCatchTime = Time.time + catchCooldown;

        
        rb.isKinematic = false;

        // Ensure the collider is solid again next fixed step
        if (makeTriggerWhileCarried) StartCoroutine(ReenableColliderNextFixed());

        rb.linearVelocity = v0;
        if (angular != Vector3.zero) rb.angularVelocity = angular;
    }



    IEnumerator ReenableColliderNextFixed()
    {
        // wait one physics step so the acorn is outside the catch trigger
        yield return new WaitForFixedUpdate();
        col.isTrigger = false;
    }

    public void RespawnToBottom()
    {
        if (!bottomRespawn) return;
        rb.isKinematic = true;
        transform.position = bottomRespawn.position + Vector3.up * 0.2f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        nextCatchTime = Time.time + 0.1f; // tiny buffer after respawn
    }
}
