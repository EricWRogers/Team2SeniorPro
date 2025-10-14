using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestMovementScript : MonoBehaviour
{
    public Rigidbody2D rb2d;
    public Vector2 movement;
    public float xSpeed = 500.0f;
    public float xDecelerate = 10.0f;
    public float xStopDeadzone = 50.0f;
    void Start()
    {
        rb2d = GetComponentInChildren<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("a"))
        {
                movement.x = (xSpeed * -1);
        }
        else if (Input.GetKeyUp("a"))
        {
            movement.x = 0;
        }
        if (Input.GetKeyDown("d"))
        {
            movement.x = (xSpeed);
        }
        else if (Input.GetKeyUp("d"))
        {
            movement.x = 0;
        }

        if (Input.GetKeyDown("w"))
        {
            
        }


        rb2d.linearVelocity = movement * Time.deltaTime;



    }
}
