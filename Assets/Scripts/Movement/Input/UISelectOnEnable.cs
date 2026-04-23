using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UISelectOnEnable : MonoBehaviour
{
    [Tooltip("The first selected UI object when this menu opens.")]
    public GameObject firstSelectedObject;

    private void OnEnable()
    {
        StartCoroutine(SelectNextFrame());
    }

    public void Reselect()
    {
        StartCoroutine(SelectNextFrame());
    }

    private IEnumerator SelectNextFrame()
    {
        yield return null;

        if (EventSystem.current == null || firstSelectedObject == null)
            yield break;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);
    }
}