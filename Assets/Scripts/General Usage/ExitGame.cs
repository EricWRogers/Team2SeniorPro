using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExitGame : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Exiting game...");
            Application.Quit();

        }

    }

}
