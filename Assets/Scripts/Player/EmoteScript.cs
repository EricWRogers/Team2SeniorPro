using UnityEngine;

public class EmoteScript : MonoBehaviour
{
    public ThirdPersonMovement playerMovement;
    public Animator playerAnimator;
    public KeyCode emoteKey = KeyCode.P;

    private bool dance = false;

    void Update()
    {
        // Press the emote key to start dancing
        if (Input.GetKeyDown(emoteKey) && !dance && playerMovement.grounded)
        {
            StartDance();
        }

        // Stop dancing if player moves
        if (dance && IsMovementInput())
        {
            StopDance();
        }
    }

    void StartDance()
    {
        dance = true;
        playerAnimator.SetBool("dance", true);

        // Disable player movement
        playerMovement.restricted = true;
        playerMovement.rb.linearVelocity = Vector3.zero;
    }

    void StopDance()
    {
        dance = false;
        playerAnimator.SetBool("dance", false);

        // Re-enable player movement
        playerMovement.restricted = false;
    }

    private bool IsMovementInput()
    {
        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }
}
