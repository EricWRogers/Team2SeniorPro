using UnityEngine;

public class JustDance : MonoBehaviour
{
    private Animator playerAnimator;
    
    void Start()
    {
        // Get the Animator component attached to the GameObject
        playerAnimator = GetComponent<Animator>();

        // Check if the Animator component was found
        if (playerAnimator == null)
        {
            Debug.LogError("Animator component not found!");
        }
    }

    void Update()
    {
        // Check for user input (e.g., pressing the space bar) to trigger the dance
        if (Input.GetKeyDown(KeyCode.Space) && playerAnimator != null)
        {
            StartTheDance();
        }
    }

    public void StartTheDance()
    {
        // Set the "StartDance" trigger in the Animator to initiate the transition
        playerAnimator.SetBool("dance", true);
    }

}
