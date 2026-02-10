using UnityEngine;

public class PlayerCarryState : MonoBehaviour
{
    public NewClimbing climbing;          
    public NewLedgeGrabbing ledgeGrab;    
    public NewThirdPlayerMovement tpm;   
    public Rigidbody playerRb;  
    [Range(0.2f, 1f)] public float carrySpeedMultiplier = 0.5f;

    public bool IsCarrying { get; private set; }

    float baseMoveSpeed;
    void Start() { if (tpm) baseMoveSpeed = tpm.walkSpeed; }

    public void SetCarrying(bool on)
    {
        IsCarrying = on;
        if (climbing) climbing.climbing = !on;
        if (ledgeGrab) ledgeGrab.holding = !on;
        if (tpm && baseMoveSpeed > 0f) tpm.walkSpeed = on ? baseMoveSpeed * carrySpeedMultiplier : baseMoveSpeed;
        
        if (playerRb) playerRb.useGravity = true;
        if (tpm) { tpm.freeze = false; tpm.restricted = false; }
    }
}
