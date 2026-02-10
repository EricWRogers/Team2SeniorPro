using TMPro;
using UnityEngine;
using System.Collections;
using SuperPupSystems;

public class UIManager : MonoBehaviour
{
    [Header("TextMeshPro UI Elements")]
    public TMP_Text speedText;
    public TMP_Text nutJumpText;
    public TMP_Text healthText;

    [Header("Player Reference")]
    //public PlayerStats playerStats;
    public MidairGrabAbility nutGrabAbility;
    public NewThirdPlayerMovement movementSpeed;

    void Update()
    {
        // Example of showing stats in text
        //speedText.text = "Speed: " + .movementSpeed.ToString("F1"); // One decimal place
        nutJumpText.text = "Nut Jumps: " + nutGrabAbility.currentNutJumps;
        //healthText.text = "Health: " + .health;
    }
}
