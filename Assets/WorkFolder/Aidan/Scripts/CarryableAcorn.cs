using UnityEngine;
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

    [Header("Bounds / Respawn")]
    public float killY = -20f;
    public Transform bottomRespawn;          
    [Tooltip("Current respawn point (set by checkpoints). If null, uses bottomRespawn.")]
    public Transform currentRespawn;

    public bool IsCarried { get; private set; }
    Transform carrierSocket;

    void Reset() { rb = GetComponent<Rigidbody>(); col = GetComponent<Collider>(); }

    void Start()
    {
        // default to bottom until a checkpoint is touched
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

    public void PickUp(Transform socket)
    {
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
        if (makeTriggerWhileCarried) StartCoroutine(ReenableColliderNextFixed());

        rb.linearVelocity = v0;
        if (angular != Vector3.zero) rb.angularVelocity = angular;
    }

    IEnumerator ReenableColliderNextFixed()
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

        nextCatchTime = Time.time + 0.15f; // tiny buffer 
    }
}
