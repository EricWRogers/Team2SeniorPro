using UnityEngine;

public class NestGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Acorn")) return;
        Debug.Log("WIN! Acorn delivered to the nest.");
        //maybe a win screen
    }
}
