using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DoorHUB : MonoBehaviour
{
    [Header("References")]
    public GameObject NutKeeperA; // door keeper alive
    public GameObject NutKeeperD; // door keeper dead
    public GameObject NutText;
    public GameObject NutText2;
    public GameObject NutPanel;
    public TMP_Text berryCountText;
    
    [Header("Settings/Keybinds")]
    public KeyCode interactKey = KeyCode.F; // Interaction Key (Default: F)

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

        // Logic to unlock Level 5 (GateKeeper)
        if (currentTotal >= berriesRequired)
        {
            if (NutKeeperA != null)
                NutKeeperA.SetActive(true); // Remove the dead door keeper and set to true (alive)

            if (NutKeeperD != null)
                NutKeeperD.SetActive(false); // Remove the dead door keeper and set to false (dead)
        }
        else
        {
            if (NutKeeperA != null)
                NutKeeperA.SetActive(false); // Ensure the dead door keeper is true

            if (NutKeeperD != null)
                NutKeeperD.SetActive(true); // Ensure the alive door keeper is false

            if (NutText2 != null && !playerInRange)
                NutText2.SetActive(false); // Hide the "door unlocked" text
        }

        // Show the "door unlocked" text only when the player is in range and has enough berries
        if (currentTotal >= berriesRequired && playerInRange)
        {
            if (NutText2 != null)
                NutText2.SetActive(true); // Show the "door unlocked" text

            if (NutText != null)
                NutText.SetActive(false); // Hide the "need more berries" text
        }

        else
        {
            if (NutText2 != null)
                NutText2.SetActive(false); // Hide the "door unlocked" text
            
            if (NutText != null && playerInRange)
                NutText.SetActive(true); // Show the "need more berries" text
        }

        // Check for interaction input to load the next level
        if (playerInRange && currentTotal >= berriesRequired && Input.GetKeyDown(interactKey))
        {
            Level_5(); // Load the next level (Level 5)
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

    // --- Level Loading ---
    public void Level_5() => LoadLevel("Level_5");

    private void LoadLevel(string levelName)
    {
        GameManager.Instance.newMap(levelName, true);
        Time.fixedDeltaTime = 1f / GameManager.Instance.frameRate;
    }
}
