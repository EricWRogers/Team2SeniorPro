using TMPro;
using UnityEngine;
using System.Collections;
using SuperPupSystems;
using System.Globalization;

public class UIManagement : MonoBehaviour
{
    [Header("TextMeshPro UI Elements")]
    //public TMP_Text speedText;
    public TMP_Text collectibleText;

    [Header("Player Reference")]
    //public PlayerStats playerStats;
    public GameObject player;

    void Start()
    {
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            //get midairgrab
            //get thirdpersonmovement
        }
    }
    void Update()
    {
        // Example of showing stats in text
        //speedText.text = "Speed: " + .movementSpeed.ToString("F1"); // One decimal place
        //nutJumpText.text = "Nut Jumps: " + nutGrabAbility.currentNutJumps;
        //healthText.text = "Health: " + .health;
        /*if (nutGrabAbility.currentNutJumps < 10)
        {
            nutJumpText.text = "0" + nutGrabAbility.currentNutJumps;
        }*/
        collectibleText.text = GameManager.Instance.collectibleCount.ToString();
        
        if(player != null)
        {
            //speedText.text = (player.GetComponent<Rigidbody>().linearVelocity.magnitude).ToString("F1", CultureInfo.InvariantCulture) + " mp/h";
        }
    }
}
