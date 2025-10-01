using UnityEngine;

public class AimCursor : MonoBehaviour
{
    public Camera aimCamera; // assign main or aim cam
    private Renderer rend;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }
    void Update()
    {
        Ray r = aimCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(r, out RaycastHit hit, 100f))
        {
            transform.position = hit.point;
            transform.rotation = Quaternion.LookRotation(hit.normal);

            if (rend) rend.enabled = true; // show
        }
        else
        {
            if (rend) rend.enabled = false; // hide, but obj stays active
        }
    }
}
