using UnityEngine;
using UnityEngine.EventSystems;

public class UIFocusGuard : MonoBehaviour
{
    public GameObject fallback;

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
        {
            if (fallback != null)
                EventSystem.current.SetSelectedGameObject(fallback);
        }
    }
}