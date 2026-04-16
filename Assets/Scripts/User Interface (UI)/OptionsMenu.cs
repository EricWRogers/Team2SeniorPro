using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    [Header("Controller/UI")]
    public GameObject firstSelectedObject;
    public GameObject parentMenuFirstSelectedObject;
    public GameObject optionsRoot;

    private void OnEnable()
    {
        if (EventSystem.current != null && firstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedObject);
        }
    }

    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);
    }

    public void CloseOptions()
    {
        if (optionsRoot != null)
            optionsRoot.SetActive(false);

        if (EventSystem.current != null && parentMenuFirstSelectedObject != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(parentMenuFirstSelectedObject);
        }
    }
}