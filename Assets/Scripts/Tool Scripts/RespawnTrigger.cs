using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            Respawn();
        }
    }
    public void Respawn()
    {
        //get player statistics singleton here, we need a singleton
    }
}
