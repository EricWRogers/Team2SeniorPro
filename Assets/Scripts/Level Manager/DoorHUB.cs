using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorHUB : MonoBehaviour
{
    public GameObject NutKeeper; // door blocker
    public GameObject NutText;
    public GameObject NutText2;
    public GameObject NutPanel;

    public TMP_Text berryCountText;
    
    public int berriesRequired = 40;

    private bool playerInRange;

    void Start()
    {
        if (NutText != null  && NutText2 != null )
        {
            NutText.SetActive(false);
            NutText2.SetActive(false);
        }

        if (NutPanel != null)
        {
            NutPanel.SetActive(false);
        }

        if (berryCountText != null)
        {
            berryCountText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        int currentTotal = GameManager.Instance.PermaBerryCount;

        if (playerInRange){
            NutPanel.SetActive(true);
            NutText.SetActive(true);
            

            // Update the UI with the current berry count
            if (berryCountText != null)
            {
                berryCountText.gameObject.SetActive(true);
                berryCountText.text = $"{currentTotal} / {berriesRequired} eaten.";
            }
        }
        else{
            NutPanel.SetActive(false);
            NutText.SetActive(false);

            if (berryCountText != null)
            {
                berryCountText.gameObject.SetActive(false);
            }
        }

        // Logic to unlock Level 5
        if (currentTotal >= berriesRequired && playerInRange)
        {
            if (NutKeeper != null)
                NutKeeper.SetActive(false); // Remove the door blocker

            if (NutText2 != null)
                NutText2.SetActive(true); // Show the "door unlocked" text

            if (NutText != null)
                NutText.SetActive(false); // Hide the "door locked" text
        }
        else
        {
            if (NutKeeper != null)
                NutKeeper.SetActive(true); // Ensure the door blocker is active

            if (NutText2 != null && !playerInRange)
                NutText2.SetActive(false); // Hide the "door unlocked" text
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
