// SCRIPT TO TEST TRIGGER OUTPUT, ONLY PUTS DEBUG LOG OUT, USE THIS SCRIPT IN COPY+PASTE TESTING
using UnityEngine;



public class TestTriggerOutput : MonoBehaviour
{
    public Collider meshCollider;
    void Awake()
    {
        if (meshCollider == null)
        {
            meshCollider = GetComponentInParent<Collider>();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.transform);
        if (other.gameObject.transform.tag == "Player")
        {
            Debug.Log("player entered!");
        }
    }
}
