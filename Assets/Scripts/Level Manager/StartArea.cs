using System;
using UnityEngine;

public class StartArea : MonoBehaviour
{
    public Timer timer;
    public Animator Time_Animator;
    public Animator Level_Animator;
    public GameObject PlayerSquirrel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer.timeRunning = false; // Ensure the timer is not running at the start
        // lock player mouse and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PlayerSquirrel == null) PlayerSquirrel = other.gameObject;

        if (PlayerSquirrel != null && other.GetComponentInParent<Collider>() != null)
        {
            Level_Animator.SetTrigger("LevelDisplay");
            Debug.Log("Player entered start area.");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!timer.timeRunning && PlayerSquirrel != null && other.GetComponentInParent<Collider>() != null)
        {
            // Start the timer when the player exits the start area
            timer.timeRunning = true;
            SoundManager.Instance.PlaySFX("Whip", 1f);
            Time_Animator.SetTrigger("TimerStart");
            Debug.Log("Player exited start area. Timer started.");
        }
    }
}
