using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoSomething : MonoBehaviour
{
    [Header("Objects & Scripts to Enable/Disable")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;
    public MonoBehaviour[] scriptsToEnable;
    public MonoBehaviour[] scriptsToDisable;

    [Header("Enable/Disable Options")]
    public bool enable = false;
    public bool disable = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Acorn"))
        {
            if (objectsToEnable != null && enable == true)
            {
                foreach (GameObject obj in objectsToEnable)
                {
                    obj.SetActive(true);
                }
            }

            if (objectsToDisable != null && disable == true)
            {
                foreach (GameObject obj in objectsToDisable)
                {
                    obj.SetActive(false);
                }
            }

            if (scriptsToEnable != null && enable == true)
            {
                foreach (MonoBehaviour script in scriptsToEnable)
                {
                    script.enabled = true;
                }
            }

            if (scriptsToDisable != null && disable == true)
            {
                foreach (MonoBehaviour script in scriptsToDisable)
                {
                    script.enabled = false;
                }
            }

        }
    }
}
