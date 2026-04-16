using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIControllerNavigation : MonoBehaviour
{
    [SerializeField] private Button firstSelectedButton;

    void Start()
    {
        // Ensure EventSystem exists
        if (EventSystem.current != null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }

    void Update()
    {
        // If nothing is selected (e.g., mouse click deselected), reselect
        if (EventSystem.current.currentSelectedGameObject == null && firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton.gameObject);
        }
    }
}
