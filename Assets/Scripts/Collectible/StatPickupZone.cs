using UnityEngine;

public class StatPickupZone : MonoBehaviour
{
    public enum PickupType
    {
        Speed,
        Jump
    }

    [Header("Pickup Settings")]
    public PickupType pickupType = PickupType.Speed;
    public float multiplier = 2f;
    public float duration = 3f;

    [Header("Refill Settings")]
    public float refillSpeed = 5f; // How many seconds of powerup is restored per real-time second

    [Header("Retrigger")]
    public float retriggerCooldown = 1f;
    private float cooldownTimer = 0f;

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        ApplyMultiplier(other);
    }

    private void OnTriggerStay(Collider other)
    {
        RefreshPowerup(other);
    }

    private void ApplyMultiplier(Collider other)
    {
        NewThirdPlayerMovement movement = other.GetComponentInParent<NewThirdPlayerMovement>();
        if (movement == null) return;

        switch (pickupType)
        {
            case PickupType.Speed:
                movement.ApplyTemporarySpeedBoost(multiplier, movement.speedBoostTimeLeft);
                break;

            case PickupType.Jump:
                movement.ApplyTemporaryJumpBoost(multiplier, movement.jumpBoostTimeLeft);
                break;
        }
    }
    
    private void TryApplyPickup(Collider other, bool firstHit)
    {
        if (cooldownTimer > 0f && firstHit) return;

        NewThirdPlayerMovement movement = other.GetComponentInParent<NewThirdPlayerMovement>();
        if (movement == null) return;

        switch (pickupType)
        {
            case PickupType.Speed:
                movement.ApplyTemporarySpeedBoost(multiplier, duration);
                break;

            case PickupType.Jump:
                movement.ApplyTemporaryJumpBoost(multiplier, duration);
                break;
        }

        cooldownTimer = retriggerCooldown;
    }

    private void RefreshPowerup(Collider other)
    {
        NewThirdPlayerMovement movement = other.GetComponentInParent<NewThirdPlayerMovement>();
        if (movement == null) return;

        switch (pickupType)
        {
            case PickupType.Speed:
                movement.speedBoostMaxTime = duration;

                movement.speedBoostTimeLeft = Mathf.MoveTowards(movement.speedBoostTimeLeft, 
                duration, refillSpeed * Time.deltaTime);
                break;

            case PickupType.Jump:
                movement.jumpBoostMaxTime = duration;

                movement.jumpBoostTimeLeft = Mathf.MoveTowards(movement.jumpBoostTimeLeft, 
                duration, refillSpeed * Time.deltaTime);
                break;
        }
    }
}