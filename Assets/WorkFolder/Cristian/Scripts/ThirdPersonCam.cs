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

    [Header("Follow")]
    public Transform cameraTarget;   //empty pivot
    public float followLerp = 12f;

    [Header("Collision")]
    public LayerMask cameraCollisionMask; // everything except Player
    public float camMinDistance = 0.6f;
    public float camMaxDistance = 4.0f;
    public float camRadius = 0.2f;   // spherecast radius
    private Vector3 desiredLocalOffset; // set in inspector

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
        if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            Vector3 yawForward = orientation.forward;
            yawForward.y = 0f;
            if (yawForward.sqrMagnitude > 0.0001f)
            {
                playerObj.forward = Vector3.Slerp(
                    playerObj.forward,
                    yawForward.normalized,
                    Time.deltaTime * rotationSpeed
                );
            }
        }

        else if (currentStyle == CameraStyle.Shoulder)
        {
            Vector3 directionToShoulderLookAt = shoulderLookAt.position - new Vector3(transform.position.x, shoulderLookAt.position.y, transform.position.z);
            orientation.forward = directionToShoulderLookAt.normalized;

            playerObj.forward = directionToShoulderLookAt.normalized;

            //Debug.Log($"[Shoulder] PlayerObj facing shoulderLookAt direction: {directionToShoulderLookAt}");
        }


        
        
        Vector3 worldDesired = cameraTarget.TransformPoint(desiredLocalOffset);
        Vector3 from = cameraTarget.position;
        Vector3 to = worldDesired;

        if (Physics.SphereCast(from, camRadius, (to - from).normalized, out RaycastHit hit, 
            camMaxDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
        {
            float d = Mathf.Clamp(hit.distance - 0.05f, camMinDistance, camMaxDistance);
            to = from + (to - from).normalized * d;
        }

        transform.position = Vector3.Lerp(transform.position, to, Time.deltaTime * followLerp);
        transform.LookAt(cameraTarget.position, Vector3.up);
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
