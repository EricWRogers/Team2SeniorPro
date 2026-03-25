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
        TryApplyPickup(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // lets the pickup work if player stands on it after cooldown
        TryApplyPickup(other);
    }

    private void TryApplyPickup(Collider other)
    {
        if (cooldownTimer > 0f) return;

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
}