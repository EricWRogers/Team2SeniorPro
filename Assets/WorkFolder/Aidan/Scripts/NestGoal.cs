using UnityEngine;
using UnityEngine.SceneManagement;

public class NestGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Acorn")) return;
        Debug.Log("WIN! Acorn delivered to the nest.");
        SceneManager.LoadScene("Kaden's Scene");
        //maybe a win screen
    }
}
