using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    public Transform shoulderLookAt;

    public GameObject thirdPersonCam;
    public GameObject shoulderCam;
    public GameObject topDownCam;
    //public GameObject fourthPersonCam; //NANI?!

    public CameraStyle currentStyle;

    public enum CameraStyle
    {
        Basic,
        Shoulder,
        Topdown
    }

    //Make Cursor Invisible
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  //makes it not move
        Cursor.visible = false;  //makes it not see - able
    }

    private void Update()
    {
        //switches cam styles
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Shoulder);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchCameraStyle(CameraStyle.Topdown);

        //rotation orientation
        Vector3 viewDirection = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDirection.normalized;

        //rotate the player object
        if(currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;
        
            if(inputDir != Vector3.zero)
            {
                Debug.Log($"[{currentStyle}] Rotating playerObj towards inputDir: {inputDir}");
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
            else
            {
                Debug.Log($"[{currentStyle}] No input, player not rotating.");
            }
        }

        else if(currentStyle == CameraStyle.Shoulder)
        {
            Vector3 directionToShoulderLookAt = shoulderLookAt.position - new Vector3(transform.position.x, shoulderLookAt.position.y, transform.position.z);
            orientation.forward = directionToShoulderLookAt.normalized;
            
            playerObj.forward = directionToShoulderLookAt.normalized;

            Debug.Log($"[Shoulder] PlayerObj facing shoulderLookAt direction: {directionToShoulderLookAt}");
        }
        

        /*if(inputDir != Vector3.zero)
        {
            Debug.Log($"[{currentStyle}] Rotating playerObj towards inputDir: {inputDir}");
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
        else
        {
            Debug.Log($"[{currentStyle}] No input, player not rotating.");
        }*/
    }

    private void SwitchCameraStyle(CameraStyle newStyle)
    {
        shoulderCam.SetActive(false);
        thirdPersonCam.SetActive(false);
        topDownCam.SetActive(false);

        if (newStyle == CameraStyle.Basic) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Shoulder) shoulderCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown) topDownCam.SetActive(true);

        Debug.Log($"Switched camera style to: {newStyle}");
        currentStyle = newStyle;
    }
}
