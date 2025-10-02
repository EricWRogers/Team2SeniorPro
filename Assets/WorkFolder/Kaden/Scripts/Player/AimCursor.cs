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

            if (Physics.Raycast(r, out RaycastHit hit, 100f))
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
