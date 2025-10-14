using UnityEngine;

public class PlayerCarryState : MonoBehaviour
{
    public Climbing climbing;          
    public LedgeGrabbing ledgeGrab;    
    public ThirdPersonMovement tpm;   
    public Rigidbody playerRb;  
    [Range(0.2f, 1f)] public float carrySpeedMultiplier = 0.5f;

    public bool IsCarrying { get; private set; }

    float baseMoveSpeed;
    void Start() { if (tpm) baseMoveSpeed = tpm.walkSpeed; }

    public void SetCarrying(bool on)
    {
        IsCarrying = on;
        if (climbing) climbing.climbEnabled = !on;
        if (ledgeGrab) ledgeGrab.ledgeGrabEnabled = !on;
        if (tpm && baseMoveSpeed > 0f) tpm.walkSpeed = on ? baseMoveSpeed * carrySpeedMultiplier : baseMoveSpeed;
        
        if (playerRb) playerRb.useGravity = true;
        if (tpm) { tpm.freeze = false; tpm.restricted = false; }
    }
}
