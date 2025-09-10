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
    public Transform shoulderLookAt;

    [Header("Camera Objects (actual cameras you enable/disable)")]
    public GameObject thirdPersonCam;
    public GameObject shoulderCam;
    public GameObject topDownCam;

    public enum CameraStyle { Basic, Shoulder, Topdown }
    public CameraStyle currentStyle = CameraStyle.Basic;

    [Header("Follow & Zoom")]
    public Transform cameraTarget;     // empty pivot on/behind shoulders
    public float defaultDistance = 6f;
    public float minDistance = 2.5f;
    public float maxDistance = 18f;
    public float heightOffset = 0.8f;
    public float followLerp = 12f;
    public float zoomLerp = 14f;
    [Tooltip("Meters per scroll notch")] public float zoomSensitivity = 1.2f;
    public bool invertScroll = false;
    [Tooltip("Keyboard = / - zoom speed (m/s)")] public float keyZoomSpeed = 6f;

    [Header("Turning")]
    public float rotationSpeed = 10f;
    public float shoulderRightOffset = 0.6f;

    [Header("Camera Collision")]
    [Tooltip("Include world layers. EXCLUDE Player.")] public LayerMask cameraCollisionMask;
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
        SwitchCameraStyle(currentStyle);
    }

    void Update()
    {
        // ---- style hotkeys ----
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchCameraStyle(CameraStyle.Basic);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchCameraStyle(CameraStyle.Shoulder);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchCameraStyle(CameraStyle.Topdown);

        // active camera for this frame
        Transform camT = ActiveCamTransform();
        if (!camT) return; // nothing to drive

        // ---- orientation yaw (camera -> player, at player height) ----
        Vector3 viewDir = player.position - new Vector3(camT.position.x, player.position.y, camT.position.z);
        if (viewDir.sqrMagnitude > 0.0001f) orientation.forward = viewDir.normalized;

        // ---- face the squirrel toward camera yaw (ignore pitch) ----
        if (currentStyle == CameraStyle.Basic || currentStyle == CameraStyle.Topdown)
        {
            Vector3 yawFwd = orientation.forward; yawFwd.y = 0f;
            if (yawFwd.sqrMagnitude > 0.0001f)
                playerObj.forward = Vector3.Slerp(playerObj.forward, yawFwd.normalized, Time.deltaTime * rotationSpeed);
        }
        else if (currentStyle == CameraStyle.Shoulder)
        {
            Vector3 dirToShoulder = shoulderLookAt.position - new Vector3(camT.position.x, shoulderLookAt.position.y, camT.position.z);
            if (dirToShoulder.sqrMagnitude > 0.0001f) orientation.forward = dirToShoulder.normalized;
            playerObj.forward = orientation.forward;
        }

        // ---- ZOOM ----
        float raw = Input.mouseScrollDelta.y;
        if (Mathf.Abs(raw) < 0.001f) raw = Input.GetAxis("Mouse ScrollWheel");
        if (invertScroll) raw = -raw;

        if (Mathf.Abs(raw) > 0.0001f)
        {
            float sign = Mathf.Sign(raw); // fixed step per notch
            targetDistance = Mathf.Clamp(targetDistance - sign * zoomSensitivity, minDistance, maxDistance);
            if (debugZoom) Debug.Log($"[Zoom] notch {raw:F3} -> target {targetDistance:F2}");
        }

        // keyboard zoom (= in, - out)
        if (Input.GetKey(KeyCode.Equals) || Input.GetKey(KeyCode.Plus))
            targetDistance = Mathf.Clamp(targetDistance - keyZoomSpeed * Time.deltaTime, minDistance, maxDistance);
        if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.Underscore))
            targetDistance = Mathf.Clamp(targetDistance + keyZoomSpeed * Time.deltaTime, minDistance, maxDistance);

        // debug snap to extremes
        if (Input.GetKeyDown(KeyCode.RightBracket)) targetDistance = maxDistance;
        if (Input.GetKeyDown(KeyCode.LeftBracket))  targetDistance = minDistance;

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.deltaTime * zoomLerp);

        // ---- place the ACTIVE camera (not this script's transform) ----
        Vector3 pivot = (cameraTarget ? cameraTarget.position : player.position) + Vector3.up * heightOffset;

        Vector3 behind = -orientation.forward; behind.y = 0f;
        if (behind.sqrMagnitude < 0.0001f) behind = -player.forward;
        behind.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, behind);
        float side = (currentStyle == CameraStyle.Shoulder) ? shoulderRightOffset : 0f;
        Vector3 pivotWithSide = pivot + right * side;

        Vector3 desired = pivotWithSide + behind * currentDistance;

        bool clamped = false;
        if (cameraCollisionMask.value != 0)
        {
            if (Physics.SphereCast(pivotWithSide, camRadius, (desired - pivotWithSide).normalized,
                                   out RaycastHit hit, currentDistance, cameraCollisionMask, QueryTriggerInteraction.Ignore))
            {
                float d = Mathf.Clamp(hit.distance - collisionBuffer, minDistance, currentDistance);
                desired = pivotWithSide + behind * d;
                clamped = true;
            }
        }

        camT.position = Vector3.Lerp(camT.position, desired, Time.deltaTime * followLerp);
        camT.LookAt(pivot, Vector3.up);

        if (debugZoom)
        {
            float actual = Vector3.Distance(camT.position, pivot);
            Debug.DrawLine(pivot, camT.position, clamped ? Color.red : Color.cyan);
            Debug.Log($"[Cam:{camT.name}] actual:{actual:F2} current:{currentDistance:F2} target:{targetDistance:F2}");
        }
    }

    Transform ActiveCamTransform()
    {
        if (currentStyle == CameraStyle.Basic  && thirdPersonCam && thirdPersonCam.activeInHierarchy) return thirdPersonCam.transform;
        if (currentStyle == CameraStyle.Shoulder && shoulderCam && shoulderCam.activeInHierarchy)     return shoulderCam.transform;
        if (currentStyle == CameraStyle.Topdown && topDownCam && topDownCam.activeInHierarchy)       return topDownCam.transform;

        // Fallback to whichever is active
        if (thirdPersonCam && thirdPersonCam.activeInHierarchy) return thirdPersonCam.transform;
        if (shoulderCam    && shoulderCam.activeInHierarchy)    return shoulderCam.transform;
        if (topDownCam     && topDownCam.activeInHierarchy)     return topDownCam.transform;

        // As a last resort, use Camera.main
        var main = Camera.main;
        return main ? main.transform : null;
    }

    public void SwitchCameraStyle(CameraStyle newStyle)
    {
        if (shoulderCam) shoulderCam.SetActive(false);
        if (thirdPersonCam) thirdPersonCam.SetActive(false);
        if (topDownCam) topDownCam.SetActive(false);

        if (newStyle == CameraStyle.Basic   && thirdPersonCam) thirdPersonCam.SetActive(true);
        if (newStyle == CameraStyle.Shoulder&& shoulderCam)    shoulderCam.SetActive(true);
        if (newStyle == CameraStyle.Topdown && topDownCam)     topDownCam.SetActive(true);

        currentStyle = newStyle;
    }
}
