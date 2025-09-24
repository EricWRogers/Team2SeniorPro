using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;   // yaw helper
    public Transform player;        // player root
    public Transform playerObj;     // visual root that should face cam yaw
    public Transform shoulderLookAt;

    
    public enum CameraStyle { Basic, Shoulder, Topdown }
    public CameraStyle currentStyle = CameraStyle.Basic;

    [Header("Follow & Zoom")]
    public Transform cameraTarget;  // empty pivot near shoulders
    public float defaultDistance = 6f;
    public float minDistance = 2.5f;
    public float maxDistance = 18f;
    public float heightOffset = 0.8f;
    public float followLerp = 12f;
    public float zoomLerp = 14f;
    [Tooltip("Meters per scroll notch")] public float zoomSensitivity = 1.2f;
    public bool invertScroll = false;
    [Tooltip("Keyboard =/- zoom speed (m/s)")] public float keyZoomSpeed = 6f;

    [Header("Turning")]
    public float rotationSpeed = 10f;
    public float shoulderRightOffset = 0.6f;

    [Header("Collision")]
    [Tooltip("Include world geometry. EXCLUDE Player.")]
    public LayerMask cameraCollisionMask = 0; // set in Inspector later
    public float camRadius = 0.25f;
    public float collisionBuffer = 0.10f;

    [Header("Debug")]
    public bool debugZoom = false;

    float targetDistance;
    float currentDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        targetDistance = currentDistance = Mathf.Clamp(defaultDistance, minDistance, maxDistance);

        if (player == null | playerObj == null | orientation == null | playerObj == null)
        {
            if (GameObject.FindGameObjectWithTag("Player").gameObject.name == "Squirrel")
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }
            else
            {
                Debug.LogError("Cant get player object!");
            }
            if (player.transform.GetChild(0).gameObject.name == "PlayerObj")
            {
                playerObj = player.transform.GetChild(0).transform;
            }
            else
            {
                Debug.LogError("Cant get playerObj object!");
            }
            if (player.transform.GetChild(1).gameObject.name == "Orientation")
            {
                orientation = player.transform.GetChild(1).transform;
            }
            else
            {
                Debug.LogError("Cant get orientation object!");
            }
            if (orientation.transform.GetChild(0).gameObject.name == "ShoulderLookAt")
            {
                shoulderLookAt = orientation.transform.GetChild(0).transform;
            }
            else
            {
                Debug.LogError("Cant get shoulderlookat object!");
            }

            //playerObj = player.transform.GetChild(0).transform;
            //orientation = playerObj.transform.GetChild(1).transform;
            //shoulderLookAt = orientation.transform.GetChild(0).transform;
        }
    }

    void Update()
    {
        // --- orientation yaw (camera -> player at player height) ---
        Vector3 camPos = transform.position;
        Vector3 viewDir = player.position - new Vector3(camPos.x, player.position.y, camPos.z);
        if (viewDir.sqrMagnitude > 0.0001f) orientation.forward = viewDir.normalized;

        // --- face the squirrel toward camera yaw (ignore pitch) ---
        if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            Vector3 yawFwd = orientation.forward; yawFwd.y = 0f;
            if (yawFwd.sqrMagnitude > 0.0001f)
                playerObj.forward = Vector3.Slerp(playerObj.forward, yawFwd.normalized, Time.deltaTime * rotationSpeed);
        }
        else if (currentStyle == CameraStyle.Shoulder)
        {
            Vector3 dirToShoulder = shoulderLookAt.position - new Vector3(camPos.x, shoulderLookAt.position.y, camPos.z);
            if (dirToShoulder.sqrMagnitude > 0.0001f) orientation.forward = dirToShoulder.normalized;
            playerObj.forward = orientation.forward;
        }

        // --- fixed-step zoom (mouse wheel) ---
        float raw = Input.mouseScrollDelta.y;
        if (Mathf.Abs(raw) < 0.001f) raw = Input.GetAxis("Mouse ScrollWheel");
        if (invertScroll) raw = -raw;

        if (Mathf.Abs(raw) > 0.0001f)
        {
            float sign = Mathf.Sign(raw);
            targetDistance = Mathf.Clamp(targetDistance - sign * zoomSensitivity, minDistance, maxDistance);
            if (debugZoom) Debug.Log($"[Zoom] notch={raw:F3} target={targetDistance:F2}");
        }
        // keyboard zoom (= in, - out)
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus))
            targetDistance = Mathf.Clamp(targetDistance - keyZoomSpeed * Time.deltaTime, minDistance, maxDistance);
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.Underscore))
            targetDistance = Mathf.Clamp(targetDistance + keyZoomSpeed * Time.deltaTime, minDistance, maxDistance);

        // debug snap: [ / ]
        if (Input.GetKeyDown(KeyCode.RightBracket)) targetDistance = maxDistance;
        if (Input.GetKeyDown(KeyCode.LeftBracket))  targetDistance = minDistance;

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomLerp);

        // --- place THIS camera ---
        Vector3 pivot = (cameraTarget ? cameraTarget.position : player.position) + Vector3.up * heightOffset;

        Vector3 behind = -orientation.forward; behind.y = 0f;
        if (behind.sqrMagnitude < 0.0001f) behind = -player.forward;
        behind.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, behind);
        float side = (currentStyle == CameraStyle.Shoulder) ? shoulderRightOffset : 0f;
        Vector3 pivotWithSide = pivot + right * side;

        Vector3 desired = pivotWithSide + behind * currentDistance;

        
        if (cameraCollisionMask.value != 0)
        {
            if (Physics.SphereCast(pivotWithSide, camRadius, (desired - pivotWithSide).normalized,
                                   out RaycastHit hit, currentDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
            {
                float d = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, currentDistance);
                desired = pivotWithSide + behind * d;
            }
        }

        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followLerp);
        transform.LookAt(pivot, Vector3.up);
    }
}
