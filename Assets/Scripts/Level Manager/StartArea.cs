using System;
using UnityEngine;

public class StartArea : MonoBehaviour
{
    public Timer timer;
    public Animator Time_Animator;
    public Animator Level_Animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer.timeRunning = false; // Ensure the timer is not running at the start
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Level_Animator.SetTrigger("LevelDisplay");
            Debug.Log("Player entered start area.");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!timer.timeRunning && other.CompareTag("Player"))
        {
            // Start the timer when the player exits the start area
            timer.timeRunning = true;
            SoundManager.Instance.PlaySFX("Whip", 1f);
            Time_Animator.SetTrigger("TimerStart");
            Debug.Log("Player exited start area. Timer started.");
        }
    }
}
