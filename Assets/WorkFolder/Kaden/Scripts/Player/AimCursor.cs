using UnityEngine;

public class AimCursor : MonoBehaviour
{
    public PlayerCarryState carryState;
    public Camera aimCamera;
    private Renderer rend;

    void Awake()
    {
        // Renders the cursor visible/invisible
        rend = GetComponentInChildren<Renderer>();
    }

    void OnEnable()
    {
        UpdateCursorState();
    }

    void Update()
    {
        UpdateCursorState();

        if (carryState == carryState.IsCarrying)
        {
            Ray r = aimCamera.ScreenPointToRay(Input.mousePosition);

            // This code will snap onto objects while aiming using Raycast
            if (Physics.Raycast(r, out RaycastHit hit, 50f))
            {
                transform.position = hit.point;
                transform.rotation = Quaternion.LookRotation(hit.normal);
            }
        }
    }

    void UpdateCursorState()
    {
        bool isCarrying = carryState.IsCarrying;
        Cursor.lockState = isCarrying ? CursorLockMode.None : CursorLockMode.Locked;
        if (rend) rend.enabled = isCarrying;
    }
}
