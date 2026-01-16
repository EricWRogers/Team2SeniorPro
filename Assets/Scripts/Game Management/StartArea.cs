using UnityEngine;

public class StartArea : MonoBehaviour
{
    public Timer timer;
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer.timeRunning = false; // Ensure the timer is not running at the start
    }


    private void OnTriggerExit(Collider other)
    {
        if (!timer.timeRunning && other.CompareTag("Player"))
        {
            // Start the timer when the player exits the start area
            timer.timeRunning = true;
            animator.SetTrigger("TimerStart");
            Debug.Log("Player exited start area. Timer started.");
        }
    }
}
