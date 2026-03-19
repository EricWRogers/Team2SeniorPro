using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorHUB : MonoBehaviour
{
    public GameObject[] NutParts; // Array to hold the nut parts
    public GameObject NutText;
    public GameObject NutText2;
    public GameObject NutPanel;

    private bool playerInRange;

    void Start()
    {
        if (NutText && NutText2 != null )
            NutText.SetActive(false);
            NutText2.SetActive(false);
        
        if (NutPanel != null)
        {
            NutPanel.SetActive(false);
        }
    }

    void Update()
    {

        if (playerInRange){
            NutPanel.SetActive(true);
            NutText.SetActive(true);
        }
        else{
            NutPanel.SetActive(false);
            NutText.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
